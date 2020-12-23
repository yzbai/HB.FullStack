#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.Properties;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Database
{
    /// <summary>
    /// 实现单表的数据库与内存映射
    /// 数据库 Write/Read Controller
    /// 要求：每张表必须有一个主键，且主键必须为int。
    /// 异常处理设置：DAL层处理DbException,其他Exception直接扔出。每个数据库执行者，只扔出异常。
    /// 异常处理，只用在写操作上。
    /// 乐观锁用在写操作上，交由各个数据库执行者实施，Version方式。
    /// 批量操作，采用事务方式，也交由各个数据库执行者实施。
    /// </summary>
    internal class DefaultDatabase : IDatabase
    {
        private readonly DatabaseCommonSettings _databaseSettings;
        private readonly IDatabaseEngine _databaseEngine;
        private readonly ITransaction _transaction;
        private readonly ILogger _logger;

        private readonly string _deletedPropertyReservedName;

        public DefaultDatabase(
            IDatabaseEngine databaseEngine,
            ITransaction transaction,
            ILogger<DefaultDatabase> logger)
        {
            _databaseSettings = databaseEngine.DatabaseSettings;
            _databaseEngine = databaseEngine;
            _transaction = transaction;
            _logger = logger;

            EngineType = _databaseEngine.EngineType;

            EntityDefFactory.Initialize(databaseEngine);

            _deletedPropertyReservedName = SqlHelper.GetReserved(nameof(Entity.Deleted), _databaseEngine.EngineType);

            if (_databaseSettings.Version < 0)
            {
                throw new ArgumentException(Resources.VersionShouldBePositiveMessage);
            }
        }

        public EngineType EngineType { get; }


        IDatabaseEngine IDatabase.DatabaseEngine => _databaseEngine;

        public IEnumerable<string> DatabaseNames => _databaseEngine.DatabaseNames;

        #region Initialize

        /// <summary>
        /// 初始化，如果在服务端，请加全局分布式锁来初始化
        /// </summary>
        /// <param name="migrations"></param>
        /// <returns></returns>
        public async Task InitializeAsync(IEnumerable<Migration>? migrations = null)
        {
            if (_databaseSettings.AutomaticCreateTable)
            {
                await AutoCreateTablesIfBrandNewAsync().ConfigureAwait(false);

                _logger.LogInformation("Database Auto Create Tables Finished.");
            }

            if (migrations != null && migrations.Any())
            {
                await MigarateAsync(migrations).ConfigureAwait(false);

                _logger.LogInformation("Database Migarate Finished.");
            }

            _logger.LogInformation("数据初始化成功！");

        }

        private async Task AutoCreateTablesIfBrandNewAsync()
        {
            foreach (string databaseName in _databaseEngine.DatabaseNames)
            {
                TransactionContext transactionContext = await _transaction.BeginTransactionAsync(databaseName, IsolationLevel.Serializable).ConfigureAwait(false);

                try
                {
                    SystemInfo sys = await GetSystemInfoAsync(databaseName, transactionContext.Transaction).ConfigureAwait(false);
                    //表明是新数据库
                    if (sys.Version == 0)
                    {
                        if (_databaseSettings.Version != 1)
                        {
                            await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                            throw new DatabaseException(ErrorCode.DatabaseTableCreateError,
                                                        "",
                                                        $"Database:{databaseName} does not exists, database Version must be 1");
                        }

                        await CreateTablesByDatabaseAsync(databaseName, transactionContext).ConfigureAwait(false);

                        await UpdateSystemVersionAsync(databaseName, 1, transactionContext.Transaction).ConfigureAwait(false);
                    }

                    await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                    if (ex is DatabaseException)
                    {
                        throw;
                    }

                    throw new DatabaseException(ErrorCode.DatabaseTableCreateError, null, $"Database:{databaseName}", ex);
                }
            }
        }

        private Task<int> CreateTableAsync(EntityDef def, TransactionContext transContext)
        {
            var command = DbCommandBuilder.CreateTableCreateCommand(EngineType, def, false);

            _logger.LogInformation($"Table创建：SQL:{command.CommandText}");

            return _databaseEngine.ExecuteCommandNonQueryAsync(transContext.Transaction, def.DatabaseName!, command);
        }

        private async Task CreateTablesByDatabaseAsync(string databaseName, TransactionContext transactionContext)
        {
            foreach (EntityDef entityDef in EntityDefFactory.GetAllDefsByDatabase(databaseName))
            {
                await CreateTableAsync(entityDef, transactionContext).ConfigureAwait(false);
            }
        }

        private async Task MigarateAsync(IEnumerable<Migration> migrations)
        {
            if (migrations != null && migrations.Any(m => m.NewVersion <= m.OldVersion))
            {
                throw new DatabaseException(ErrorCode.DatabaseMigrateError, "", Resources.MigrationVersionErrorMessage);
            }

            foreach (string databaseName in _databaseEngine.DatabaseNames)
            {
                TransactionContext transactionContext = await _transaction.BeginTransactionAsync(databaseName, IsolationLevel.Serializable).ConfigureAwait(false);

                try
                {
                    SystemInfo sys = await GetSystemInfoAsync(databaseName, transactionContext.Transaction).ConfigureAwait(false);

                    if (sys.Version < _databaseSettings.Version)
                    {
                        if (migrations == null)
                        {
                            throw new DatabaseException(ErrorCode.DatabaseMigrateError, "", $"Lack Migrations for {sys.DatabaseName}");
                        }

                        IOrderedEnumerable<Migration> curOrderedMigrations = migrations
                            .Where(m => m.TargetSchema.Equals(sys.DatabaseName, GlobalSettings.ComparisonIgnoreCase))
                            .OrderBy(m => m.OldVersion);

                        if (curOrderedMigrations == null)
                        {
                            throw new DatabaseException(ErrorCode.DatabaseMigrateError, "", $"Lack Migrations for {sys.DatabaseName}");
                        }

                        if (!CheckMigration(sys.Version, _databaseSettings.Version, curOrderedMigrations))
                        {
                            throw new DatabaseException(ErrorCode.DatabaseMigrateError, "", $"Can not perform Migration on ${sys.DatabaseName}, because the migrations provided is not sufficient.");
                        }

                        foreach (Migration migration in curOrderedMigrations)
                        {
                            EngineCommand command = new EngineCommand(migration.SqlStatement);

                            await _databaseEngine.ExecuteCommandNonQueryAsync(transactionContext.Transaction, databaseName, command).ConfigureAwait(false);
                        }

                        await UpdateSystemVersionAsync(sys.DatabaseName, _databaseSettings.Version, transactionContext.Transaction).ConfigureAwait(false);
                    }

                    await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                    if (ex is DatabaseException)
                    {
                        throw;
                    }

                    throw new DatabaseException(ErrorCode.DatabaseMigrateError, null, $"Database:{databaseName}", ex);
                }
            }
        }

        private static bool CheckMigration(int startVersion, int endVersion, IOrderedEnumerable<Migration> curOrderedMigrations)
        {
            int curVersion = curOrderedMigrations.ElementAt(0).OldVersion;

            if (curVersion != startVersion) { return false; }

            foreach (Migration migration in curOrderedMigrations)
            {
                if (curVersion != migration.OldVersion)
                {
                    return false;
                }

                curVersion = migration.NewVersion;
            }

            return curVersion == endVersion;
        }

        #endregion

        #region SystemInfo

        private async Task<bool> IsTableExistsAsync(string databaseName, string tableName, IDbTransaction transaction)
        {
            var command = DbCommandBuilder.CreateIsTableExistCommand(EngineType, databaseName, tableName);

            object result = await _databaseEngine.ExecuteCommandScalarAsync(transaction, databaseName, command, true).ConfigureAwait(false);

            return Convert.ToBoolean(result, GlobalSettings.Culture);
        }

        public async Task<SystemInfo> GetSystemInfoAsync(string databaseName, IDbTransaction transaction)
        {
            bool isExisted = await IsTableExistsAsync(databaseName, SystemInfoNames.SystemInfoTableName, transaction).ConfigureAwait(false);

            if (!isExisted)
            {
                return new SystemInfo(databaseName) { Version = 0 };
            }

            var command = DbCommandBuilder.CreateSystemInfoRetrieveCommand(EngineType);

            using IDataReader reader = await _databaseEngine.ExecuteCommandReaderAsync(transaction, databaseName, command, false).ConfigureAwait(false);

            SystemInfo systemInfo = new SystemInfo(databaseName);

            while (reader.Read())
            {
                systemInfo.Set(reader["Name"].ToString(), reader["Value"].ToString());
            }

            return systemInfo;
        }

        public async Task UpdateSystemVersionAsync(string databaseName, int version, IDbTransaction transaction)
        {
            var command = DbCommandBuilder.CreateSystemVersionUpdateCommand(EngineType, databaseName, version);

            await _databaseEngine.ExecuteCommandNonQueryAsync(transaction, databaseName, command).ConfigureAwait(false);
        }

        #endregion

        #region 条件构造

        public FromExpression<T> From<T>() where T : DatabaseEntity, new()
        {
            return new FromExpression<T>(EngineType);
        }

        public WhereExpression<T> Where<T>() where T : DatabaseEntity, new()
        {
            return new WhereExpression<T>(EngineType);
        }

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DatabaseEntity, new()
        {
            return new WhereExpression<T>(EngineType).Where(sqlFilter, filterParams);
        }

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DatabaseEntity, new()
        {
            return new WhereExpression<T>(EngineType).Where(predicate);
        }

        #endregion

        #region 单表查询 From, Where

        /// <returns></returns>
        public async Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            IEnumerable<T> lst = await RetrieveAsync(fromCondition, whereCondition, transContext).ConfigureAwait(false);

            if (lst.IsNullOrEmpty())
            {
                return null;
            }

            if (lst.Count() > 1)
            {
                string detail = $"Scalar retrieve return more than one result. From:{fromCondition}, Where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseFoundTooMuch, typeof(T).FullName, detail);
            }

            return lst.ElementAt(0);
        }

        public async Task<IEnumerable<TSelect>> RetrieveAsync<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, TransactionContext? transContext = null)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TWhere>(EngineType);
            }

            EntityDef selectDef = EntityDefFactory.GetDef<TSelect>()!;
            EntityDef fromDef = EntityDefFactory.GetDef<TFrom>()!;
            EntityDef whereDef = EntityDefFactory.GetDef<TWhere>()!;

            whereCondition.And($"{whereDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {selectDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {fromDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType, fromCondition, whereCondition, selectDef);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, selectDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);

                return reader.ToEntities<TSelect>(_databaseEngine.EngineType, selectDef);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"from:{fromCondition}, where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseError, selectDef.EntityFullName, detail, ex);
            }
        }

        public async Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<T>(EngineType);
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            whereCondition.And($"{entityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand(EngineType, entityDef, fromCondition, whereCondition);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToEntities<T>(_databaseEngine.EngineType, entityDef);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $" from:{fromCondition}, where:{whereCondition}";

                throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex);
            }
        }

        public Task<IEnumerable<T>> PageAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<T>(EngineType);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync(fromCondition, whereCondition, transContext);
        }

        public async Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<T>(EngineType);
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            whereCondition.And($"{entityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                var command = DbCommandBuilder.CreateCountCommand(EngineType, fromCondition, whereCondition);
                object countObj = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return Convert.ToInt32(countObj, GlobalSettings.Culture);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"from:{fromCondition}, where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex);
            }
        }

        #endregion

        #region 单表查询, Where

        public Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(null, null, transContext);
        }

        public Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> PageAsync<T>(WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            return PageAsync(null, whereCondition, pageNumber, perPageCount, transContext);
        }

        public Task<IEnumerable<T>> PageAsync<T>(long pageNumber, long perPageCount, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            return PageAsync<T>(null, null, pageNumber, perPageCount, transContext);
        }

        public Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            return CountAsync(null, condition, transContext);
        }

        public Task<long> CountAsync<T>(TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            return CountAsync<T>(null, null, transContext);
        }

        #endregion

        #region 单表查询, Expression Where

        public Task<T?> ScalarAsync<T>(long id, TransactionContext? transContext)
            where T : IdDatabaseEntity, new()
        {
            WhereExpression<T> where = Where<T>($"{SqlHelper.GetReserved(nameof(IdDatabaseEntity.Id), EngineType)}={{0}}", id);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(string guid, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> where = Where<T>($"{SqlHelper.GetReserved(nameof(GuidEntity.Guid), EngineType)}={{0}}", guid);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> PageAsync<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(EngineType);

            return PageAsync(null, whereCondition, pageNumber, perPageCount, transContext);
        }

        public Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return CountAsync(null, whereCondition, transContext);
        }

        #endregion

        #region 双表查询

        public async Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(EngineType);
            }

            EntityDef sourceEntityDef = EntityDefFactory.GetDef<TSource>()!;
            EntityDef targetEntityDef = EntityDefFactory.GetDef<TTarget>()!;

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false);
                    break;

                case SqlJoinType.RIGHT:
                    whereCondition.And($"{targetEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And<TTarget>(t => t.Deleted == false);
                    break;

                case SqlJoinType.INNER:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;

                case SqlJoinType.FULL:
                    break;

                case SqlJoinType.CROSS:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;
            }

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand<TSource, TTarget>(EngineType, fromCondition, whereCondition, sourceEntityDef, targetEntityDef);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, sourceEntityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToEntities<TSource, TTarget>(_databaseEngine.EngineType, sourceEntityDef, targetEntityDef);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"from:{fromCondition}, where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseError, sourceEntityDef.EntityFullName, detail, ex);
            }
        }

        public Task<IEnumerable<Tuple<TSource, TTarget?>>> PageAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(EngineType);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext);
        }

        public async Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            IEnumerable<Tuple<TSource, TTarget?>> lst = await RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext).ConfigureAwait(false);

            if (lst.IsNullOrEmpty())
            {
                return null;
            }

            if (lst.Count() > 1)
            {
                string message = $"Scalar retrieve return more than one result. From:{fromCondition}, Where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseFoundTooMuch, typeof(TSource).FullName, message);
            }

            return lst.ElementAt(0);
        }

        #endregion

        #region 三表查询

        public async Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(EngineType);
            }

            EntityDef sourceEntityDef = EntityDefFactory.GetDef<TSource>()!;
            EntityDef targetEntityDef1 = EntityDefFactory.GetDef<TTarget1>()!;
            EntityDef targetEntityDef2 = EntityDefFactory.GetDef<TTarget2>()!;

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false);
                    break;

                case SqlJoinType.RIGHT:
                    whereCondition.And($"{targetEntityDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And<TTarget2>(t => t.Deleted == false);
                    break;

                case SqlJoinType.INNER:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetEntityDef1.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetEntityDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;

                case SqlJoinType.FULL:
                    break;

                case SqlJoinType.CROSS:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetEntityDef1.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetEntityDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;
            }

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(EngineType, fromCondition, whereCondition, sourceEntityDef, targetEntityDef1, targetEntityDef2);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, sourceEntityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToEntities<TSource, TTarget1, TTarget2>(_databaseEngine.EngineType, sourceEntityDef, targetEntityDef1, targetEntityDef2);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"from:{fromCondition}, where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseError, sourceEntityDef.EntityFullName, detail, ex);
            }
        }

        public Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> PageAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(EngineType);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext);
        }

        public async Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>> lst = await RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext).ConfigureAwait(false);

            if (lst.IsNullOrEmpty())
            {
                return null;
            }

            if (lst.Count() > 1)
            {
                string message = $"Scalar retrieve return more than one result. From:{fromCondition}, Where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseFoundTooMuch, typeof(TSource).FullName, message);
            }

            return lst.ElementAt(0);
        }

        #endregion

        #region 单体更改(Write)

        /// <summary>
        /// 增加,并且item被重新赋值，反应Version变化
        /// </summary>
        public async Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIf.NotValid(item);

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Entity:{SerializeUtil.ToJson(item)}");
            }

            try
            {
                item.Version = 0;
                item.LastUser = lastUser;
                item.LastTime = TimeUtil.UtcNow;

                var command = DbCommandBuilder.CreateAddCommand(EngineType, entityDef, item);

                object rt = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName!, command, true).ConfigureAwait(false);

                if (entityDef.IsIdAutoIncrement)
                {
                    ((AutoIcrementIdEntity)(object)item).Id = Convert.ToInt64(rt, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                if (transContext != null || ex is DatabaseEngineException)
                {
                    item.Version = -1;
                }

                if (!(ex is DatabaseException))
                {
                    string detail = $"Item:{SerializeUtil.ToJson(item)}";
                    throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex); ;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Version控制,反应Version变化
        /// </summary>
        public async Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIf.NotValid(item);

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Entity:{SerializeUtil.ToJson(item)}");
            }

            try
            {
                item.Deleted = true;
                item.Version++;
                item.LastUser = lastUser;
                item.LastTime = TimeUtil.UtcNow;

                var command = DbCommandBuilder.CreateDeleteCommand(EngineType, entityDef, item);

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw new DatabaseEngineException(ErrorCode.DatabaseNotFound, entityDef.EntityFullName, $"Entity:{SerializeUtil.ToJson(item)}");
                }

                throw new DatabaseException(ErrorCode.DatabaseFoundTooMuch, entityDef.EntityFullName, $"Multiple Rows Affected instead of one. Something go wrong. Entity:{SerializeUtil.ToJson(item)}");
            }
            catch (Exception ex)
            {
                if (transContext != null || ex is DatabaseEngineException)
                {
                    item.Deleted = false;
                    item.Version--;
                }

                if (!(ex is DatabaseException))
                {
                    string detail = $"Item:{SerializeUtil.ToJson(item)}";
                    throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex); ;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///  修改，建议每次修改前先select，并放置在一个事务中。
        ///  版本控制，如果item中Version未赋值，会无法更改
        ///  反应Version变化
        /// </summary>
        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIf.NotValid(item);

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Entity:{SerializeUtil.ToJson(item)}");
            }

            try
            {
                item.LastUser = lastUser;
                item.LastTime = TimeUtil.UtcNow;
                item.Version++;

                var command = DbCommandBuilder.CreateUpdateCommand(EngineType, entityDef, item);
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw new DatabaseEngineException(ErrorCode.DatabaseNotFound, entityDef.EntityFullName, $"Entity:{SerializeUtil.ToJson(item)}");
                }

                throw new DatabaseException(ErrorCode.DatabaseFoundTooMuch, entityDef.EntityFullName, $"Multiple Rows Affected instead of one. Something go wrong. Entity:{SerializeUtil.ToJson(item)}");
            }
            catch (Exception ex)
            {
                if (transContext != null || ex is DatabaseEngineException)
                {
                    item.Version--;
                }

                if (!(ex is DatabaseException))
                {
                    string detail = $"Item:{SerializeUtil.ToJson(item)}";
                    throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex); ;
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion

        #region 批量更改(Write)

        /// <summary>
        /// BatchAddAsync，反应Version变化
        /// </summary>
        public async Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            ThrowIf.NotValid(items);

            if (!items.Any())
            {
                return new List<object>();
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Items:{SerializeUtil.ToJson(items)}");
            }

            try
            {
                items.ForEach(item =>
                {
                    item.Version = 0;
                    item.LastUser = lastUser;
                    item.LastTime = TimeUtil.UtcNow;
                });

                IList<object> newIds = new List<object>();

                var command = DbCommandBuilder.CreateBatchAddCommand(EngineType, entityDef, items);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext.Transaction,
                    entityDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                if (entityDef.IsIdAutoIncrement)
                {
                    while (reader.Read())
                    {
                        newIds.Add(reader.GetValue(0));
                    }

                    int num = 0;

                    foreach (var item in items)
                    {
                        ((AutoIcrementIdEntity)(object)item).Id = Convert.ToInt64(newIds[num++], CultureInfo.InvariantCulture);
                    }
                }
                else if (entityDef.IsIdGuid)
                {
                    foreach (var item in items)
                    {
                        newIds.Add(((GuidEntity)(object)item).Guid);
                    }
                }
                else
                {
                    foreach (var item in items)
                    {
                        newIds.Add(((IdGenEntity)(object)item).Id);
                    }
                }

                return newIds;
            }
            catch (Exception ex)
            {
                items.ForEach(item =>
                {
                    item.Version = -1;
                });

                if (!(ex is DatabaseException))
                {
                    string detail = $"Items:{SerializeUtil.ToJson(items)}";
                    throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 批量更改，反应Version变化
        /// </summary>
        public async Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            ThrowIf.NotValid(items);

            if (!items.Any())
            {
                return;
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Items:{SerializeUtil.ToJson(items)}");
            }

            try
            {
                items.ForEach(item =>
                {
                    item.Version++;
                    item.LastUser = lastUser;
                    item.LastTime = TimeUtil.UtcNow;
                });

                var command = DbCommandBuilder.CreateBatchUpdateCommand(EngineType, entityDef, items);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext.Transaction,
                    entityDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw new DatabaseException(ErrorCode.DatabaseNotFound, entityDef.EntityFullName, $"BatchUpdate wrong, not found the {count}th data item. Items:{SerializeUtil.ToJson(items)}");
                    }

                    count++;
                }

                if (count != items.Count())
                {
                    throw new DatabaseException(ErrorCode.DatabaseNotFound, entityDef.EntityFullName, $"BatchUpdate wrong number return. Some data item not found. Items:{SerializeUtil.ToJson(items)}");
                }
            }
            catch (Exception ex)
            {
                items.ForEach(item =>
                {
                    item.Version--;
                });

                if (!(ex is DatabaseException))
                {
                    string detail = $"Items:{SerializeUtil.ToJson(items)}";
                    throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// BatchDeleteAsync, 反应version的变化
        /// </summary>
        public async Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            ThrowIf.NotValid(items);

            if (!items.Any())
            {
                return;
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Items:{SerializeUtil.ToJson(items)}");
            }

            try
            {
                items.ForEach(item =>
                {
                    item.Version++;
                    item.Deleted = true;
                    item.LastUser = lastUser;
                    item.LastTime = TimeUtil.UtcNow;
                });

                var command = DbCommandBuilder.CreateBatchDeleteCommand(EngineType, entityDef, items);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext.Transaction,
                    entityDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int affected = reader.GetInt32(0);

                    if (affected != 1)
                    {
                        throw new DatabaseException(ErrorCode.DatabaseNotFound, entityDef.EntityFullName, $"BatchDelete wrong, not found the {" + count + "}th data item. Items:{SerializeUtil.ToJson(items)}");
                    }

                    count++;
                }

                if (count != items.Count())
                {
                    throw new DatabaseException(ErrorCode.DatabaseNotFound, entityDef.EntityFullName, $"BatchDelete wrong number return. Some data item not found. Items:{SerializeUtil.ToJson(items)}");
                }
            }
            catch (Exception ex)
            {
                items.ForEach(item =>
                {
                    item.Version--;
                    item.Deleted = false;
                });

                if (!(ex is DatabaseException))
                {
                    string detail = $"Items:{SerializeUtil.ToJson(items)}";
                    throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion
    }
}