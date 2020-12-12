#nullable enable

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Properties;
using HB.FullStack.Database.SQL;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
        private readonly IDatabaseEntityDefFactory _entityDefFactory;
        private readonly IDbCommandBuilder _sqlBuilder;
        private readonly ITransaction _transaction;
        private readonly ILogger _logger;
        private readonly IDistributedLockManager _lockManager;

        private readonly string _deletedReservedName;

        public DefaultDatabase(
            IDatabaseEngine databaseEngine,
            IDatabaseEntityDefFactory modelDefFactory,
            IDbCommandBuilder sqlBuilder,
            ITransaction transaction,
            ILogger<DefaultDatabase> logger,
            IDistributedLockManager lockManager)
        {
            _databaseSettings = databaseEngine.DatabaseSettings;
            _databaseEngine = databaseEngine;
            _entityDefFactory = modelDefFactory;
            _sqlBuilder = sqlBuilder;
            _transaction = transaction;
            _logger = logger;
            _lockManager = lockManager;

            _deletedReservedName = SqlHelper.GetReserved(nameof(Entity.Deleted), _databaseEngine.EngineType);

            if (_databaseSettings.Version < 0)
            {
                throw new ArgumentException(Resources.VersionShouldBePositiveMessage);
            }
        }

        #region Initialize

        public async Task InitializeAsync(IEnumerable<Migration>? migrations = null)
        {
            _logger.LogDebug($"开始初始化数据库:{_databaseEngine.GetDatabaseNames().ToJoinedString(",")}");

            using IDistributedLock distributedLock = await _lockManager.LockAsync(
                resources: _databaseEngine.GetDatabaseNames(),
                expiryTime: TimeSpan.FromMinutes(5),
                waitTime: TimeSpan.FromMinutes(10)).ConfigureAwait(false);

            _logger.LogDebug($"获取了初始化数据库的锁:{_databaseEngine.GetDatabaseNames().ToJoinedString(",")}");

            try
            {
                if (!distributedLock.IsAcquired)
                {
                    ThrowIfDatabaseInitLockNotGet(_databaseEngine.GetDatabaseNames());
                }

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
            finally
            {
                distributedLock.Dispose();
            }
        }

        private async Task AutoCreateTablesIfBrandNewAsync()
        {
            foreach (string databaseName in _databaseEngine.GetDatabaseNames())
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

        private Task<int> CreateTableAsync(DatabaseEntityDef def, TransactionContext transContext)
        {
            IDbCommand command = _sqlBuilder.CreateTableCreateCommand(def, false);

            _logger.LogInformation($"Table创建：SQL:{command.CommandText}");

            return _databaseEngine.ExecuteCommandNonQueryAsync(transContext.Transaction, def.DatabaseName!, command);
        }

        private async Task CreateTablesByDatabaseAsync(string databaseName, TransactionContext transactionContext)
        {
            foreach (DatabaseEntityDef entityDef in _entityDefFactory.GetAllDefsByDatabase(databaseName))
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

            foreach (string databaseName in _databaseEngine.GetDatabaseNames())
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
                            IDbCommand command = _databaseEngine.CreateTextCommand(migration.SqlStatement);
                            await _databaseEngine.ExecuteCommandNonQueryAsync(transactionContext.Transaction, databaseName, command).ConfigureAwait(false);
                        }

                        await UpdateSystemVersionAsync(sys.DatabaseName, _databaseSettings.Version, transactionContext.Transaction).ConfigureAwait(false);
                    }

                    await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                    //throw new DatabaseException(DatabaseError.MigrateError, "", $"Migration Failed at Database:{databaseName}", ex);

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

        public DatabaseEngineType EngineType => _databaseEngine.EngineType;

        #endregion

        #region SystemInfo

        private async Task<bool> IsTableExistsAsync(string databaseName, string tableName, IDbTransaction transaction)
        {
            using IDbCommand command = _sqlBuilder.CreateIsTableExistCommand(databaseName, tableName);

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

            using IDbCommand command = _sqlBuilder.CreateSystemInfoRetrieveCommand();

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
            using IDbCommand command = _sqlBuilder.CreateSystemVersionUpdateCommand(databaseName, version);

            await _databaseEngine.ExecuteCommandNonQueryAsync(transaction, databaseName, command).ConfigureAwait(false);
        }

        #endregion

        #region 条件构造

        public FromExpression<T> From<T>() where T : Entity, new()
        {
            return _sqlBuilder.NewFrom<T>();
        }

        public WhereExpression<T> Where<T>() where T : Entity, new()
        {
            return _sqlBuilder.NewWhere<T>();
        }

        #endregion

        #region 单表查询 From, Where

        /// <returns></returns>
        public async Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : Entity, new()
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
            where TSelect : Entity, new()
            where TFrom : Entity, new()
            where TWhere : Entity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TWhere>();
            }

            DatabaseEntityDef selectDef = _entityDefFactory.GetDef<TSelect>();
            DatabaseEntityDef fromDef = _entityDefFactory.GetDef<TFrom>();
            DatabaseEntityDef whereDef = _entityDefFactory.GetDef<TWhere>();

            whereCondition.And($"{whereDef.DbTableReservedName}.{_deletedReservedName}=0 and {selectDef.DbTableReservedName}.{_deletedReservedName}=0 and {fromDef.DbTableReservedName}.{_deletedReservedName}=0");

            IList<TSelect> result;
            IDbCommand? command = null;
            IDataReader? reader = null;

            try
            {
                command = _sqlBuilder.CreateRetrieveCommand<TSelect, TFrom, TWhere>(fromCondition, whereCondition, selectDef);

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, selectDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);

                result = reader.ToEntities<TSelect>(selectDef);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"from:{fromCondition}, where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseError, selectDef.EntityFullName, detail, ex);
            }
            finally
            {
                reader?.Dispose();
                command?.Dispose();
            }

            return result;
        }

        public async Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : Entity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            whereCondition.And($"{entityDef.DbTableReservedName}.{_deletedReservedName}=0");

            IList<T> result;
            IDbCommand? command = null;
            IDataReader? reader = null;

            try
            {
                command = _sqlBuilder.CreateRetrieveCommand(entityDef, fromCondition, whereCondition);

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                result = reader.ToEntities<T>(entityDef);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $" from:{fromCondition}, where:{whereCondition}";

                throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex);
            }
            finally
            {
                reader?.Dispose();
                command?.Dispose();
            }

            return result;
        }

        public Task<IEnumerable<T>> PageAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where T : Entity, new()
        {

            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync(fromCondition, whereCondition, transContext);
        }

        public async Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : Entity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            whereCondition.And($"{entityDef.DbTableReservedName}.{_deletedReservedName}=0");

            long count;

            try
            {
                IDbCommand command = _sqlBuilder.CreateCountCommand(fromCondition, whereCondition);
                object countObj = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                count = Convert.ToInt32(countObj, GlobalSettings.Culture);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"from:{fromCondition}, where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, detail, ex);
            }

            return count;
        }

        #endregion

        #region 单表查询, Where

        public Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext)
            where T : Entity, new()
        {
            return RetrieveAsync<T>(null, null, transContext);
        }

        public Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : Entity, new()
        {
            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : Entity, new()
        {
            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> PageAsync<T>(WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where T : Entity, new()
        {
            return PageAsync(null, whereCondition, pageNumber, perPageCount, transContext);
        }

        public Task<IEnumerable<T>> PageAsync<T>(long pageNumber, long perPageCount, TransactionContext? transContext)
            where T : Entity, new()
        {
            return PageAsync<T>(null, null, pageNumber, perPageCount, transContext);
        }

        public Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext)
            where T : Entity, new()
        {
            return CountAsync(null, condition, transContext);
        }

        public Task<long> CountAsync<T>(TransactionContext? transContext)
            where T : Entity, new()
        {
            return CountAsync<T>(null, null, transContext);
        }

        #endregion

        #region 单表查询, Expression Where

        public Task<T?> ScalarAsync<T>(long id, TransactionContext? transContext)
            where T : Entity, new()
        {
            WhereExpression<T> where = Where<T>().Where("Id={0}", id);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : Entity, new()
        {
            WhereExpression<T> whereCondition = Where<T>();
            whereCondition.Where(whereExpr);

            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext)
            where T : Entity, new()
        {
            WhereExpression<T> whereCondition = Where<T>();
            whereCondition.Where(whereExpr);

            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> PageAsync<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, TransactionContext? transContext)
            where T : Entity, new()
        {
            WhereExpression<T> whereCondition = Where<T>();

            return PageAsync(null, whereCondition, pageNumber, perPageCount, transContext);
        }

        /// <summary>
        /// CountAsync
        /// </summary>
        /// <param name="whereExpr"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>

        public Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext)
            where T : Entity, new()
        {
            WhereExpression<T> whereCondition = Where<T>();
            whereCondition.Where(whereExpr);

            return CountAsync(null, whereCondition, transContext);
        }

        #endregion

        #region 双表查询

        public async Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget : Entity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            DatabaseEntityDef sourceEntityDef = _entityDefFactory.GetDef<TSource>();
            DatabaseEntityDef targetEntityDef = _entityDefFactory.GetDef<TTarget>();

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false);
                    break;
                case SqlJoinType.RIGHT:
                    whereCondition.And($"{targetEntityDef.DbTableReservedName}.{_deletedReservedName}=0");
                    //whereCondition.And<TTarget>(t => t.Deleted == false);
                    break;
                case SqlJoinType.INNER:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedReservedName}=0 and {targetEntityDef.DbTableReservedName}.{_deletedReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;
                case SqlJoinType.FULL:
                    break;
                case SqlJoinType.CROSS:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedReservedName}=0 and {targetEntityDef.DbTableReservedName}.{_deletedReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;
            }

            IList<Tuple<TSource, TTarget?>> result;
            IDbCommand? command = null;
            IDataReader? reader = null;


            try
            {
                command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget>(fromCondition, whereCondition, sourceEntityDef, targetEntityDef);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, sourceEntityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                result = reader.ToEntities<TSource, TTarget>(sourceEntityDef, targetEntityDef);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"from:{fromCondition}, where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseError, sourceEntityDef.EntityFullName, detail, ex);
            }
            finally
            {
                reader?.Dispose();
                command?.Dispose();
            }

            return result;
        }

        public Task<IEnumerable<Tuple<TSource, TTarget?>>> PageAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget : Entity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext);
        }

        public async Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget : Entity, new()
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
            where TSource : Entity, new()
            where TTarget1 : Entity, new()
            where TTarget2 : Entity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            DatabaseEntityDef sourceEntityDef = _entityDefFactory.GetDef<TSource>();
            DatabaseEntityDef targetEntityDef1 = _entityDefFactory.GetDef<TTarget1>();
            DatabaseEntityDef targetEntityDef2 = _entityDefFactory.GetDef<TTarget2>();

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false);
                    break;
                case SqlJoinType.RIGHT:
                    whereCondition.And($"{targetEntityDef2.DbTableReservedName}.{_deletedReservedName}=0");
                    //whereCondition.And<TTarget2>(t => t.Deleted == false);
                    break;
                case SqlJoinType.INNER:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedReservedName}=0 and {targetEntityDef1.DbTableReservedName}.{_deletedReservedName}=0 and {targetEntityDef2.DbTableReservedName}.{_deletedReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;
                case SqlJoinType.FULL:
                    break;
                case SqlJoinType.CROSS:
                    whereCondition.And($"{sourceEntityDef.DbTableReservedName}.{_deletedReservedName}=0 and {targetEntityDef1.DbTableReservedName}.{_deletedReservedName}=0 and {targetEntityDef2.DbTableReservedName}.{_deletedReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;
            }


            IList<Tuple<TSource, TTarget1?, TTarget2?>> result;
            IDbCommand? command = null;
            IDataReader? reader = null;

            try
            {
                command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, sourceEntityDef, targetEntityDef1, targetEntityDef2);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, sourceEntityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                result = reader.ToEntities<TSource, TTarget1, TTarget2>(sourceEntityDef, targetEntityDef1, targetEntityDef2);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"from:{fromCondition}, where:{whereCondition}";
                throw new DatabaseException(ErrorCode.DatabaseError, sourceEntityDef.EntityFullName, detail, ex);
            }
            finally
            {
                reader?.Dispose();
                command?.Dispose();
            }
            return result;
        }

        public Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> PageAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget1 : Entity, new()
            where TTarget2 : Entity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext);
        }

        public async Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget1 : Entity, new()
            where TTarget2 : Entity, new()
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
        public async Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new()
        {
            ThrowIf.NotValid(item);

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Entity:{SerializeUtil.ToJson(item)}");
            }

            IDbCommand? dbCommand = null;
            IDataReader? reader = null;

            try
            {
                item.Version = 0;
                item.LastUser = lastUser;
                item.LastTime = TimeUtil.UtcNow;

                dbCommand = _sqlBuilder.CreateAddCommand(entityDef, item);

                object rt = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName!, dbCommand, true).ConfigureAwait(false);

                item.Id = Convert.ToInt64(rt, CultureInfo.InvariantCulture);
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
            finally
            {
                reader?.Dispose();
                dbCommand?.Dispose();
            }
        }

        /// <summary>
        /// Version控制,反应Version变化
        /// </summary>
        public async Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new()
        {
            ThrowIf.NotValid(item);

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

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

                IDbCommand dbCommand = _sqlBuilder.CreateDeleteCommand(entityDef, item);

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName!, dbCommand).ConfigureAwait(false);

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
        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new()
        {
            ThrowIf.NotValid(item);

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Entity:{SerializeUtil.ToJson(item)}");
            }

            try
            {
                item.LastUser = lastUser;
                item.LastTime = TimeUtil.UtcNow;
                item.Version++;

                IDbCommand dbCommand = _sqlBuilder.CreateUpdateCommand(entityDef, item);
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName!, dbCommand).ConfigureAwait(false);

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
        public async Task<IEnumerable<long>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new()
        {
            ThrowIf.NotValid(items);

            if (!items.Any())
            {
                return new List<long>();
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Items:{SerializeUtil.ToJson(items)}");
            }

            IDbCommand? dbCommand = null;
            IDataReader? reader = null;

            try
            {
                items.ForEach(item =>
                {
                    item.Version = 0;
                    item.LastUser = lastUser;
                    item.LastTime = TimeUtil.UtcNow;
                });

                IList<long> newIds = new List<long>();

                dbCommand = _sqlBuilder.CreateBatchAddCommand(entityDef, items);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext.Transaction,
                    entityDef.DatabaseName!,
                    dbCommand,
                    true).ConfigureAwait(false);

                while (reader.Read())
                {
                    newIds.Add(reader.GetInt64(0));
                }

                for (int i = 0; i < items.Count(); ++i)
                {
                    T item = items.ElementAt(i);
                    item.Id = newIds[i];
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
            finally
            {
                reader?.Dispose();
                dbCommand?.Dispose();
            }
        }

        /// <summary>
        /// 批量更改，反应Version变化
        /// </summary>
        public async Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new()
        {
            ThrowIf.NotValid(items);

            if (!items.Any())
            {
                return;
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Items:{SerializeUtil.ToJson(items)}");
            }

            IDbCommand? dbCommand = null;
            IDataReader? reader = null;

            try
            {
                items.ForEach(item =>
                {
                    item.Version++;
                    item.LastUser = lastUser;
                    item.LastTime = TimeUtil.UtcNow;
                });

                dbCommand = _sqlBuilder.CreateBatchUpdateCommand(entityDef, items);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext.Transaction,
                    entityDef.DatabaseName!,
                    dbCommand,
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
            finally
            {
                reader?.Dispose();
                dbCommand?.Dispose();
            }
        }

        /// <summary>
        /// BatchDeleteAsync, 反应version的变化
        /// </summary>
        public async Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new()
        {
            ThrowIf.NotValid(items);

            if (!items.Any())
            {
                return;
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName, $"Items:{SerializeUtil.ToJson(items)}");
            }

            IDbCommand? dbCommand = null;
            IDataReader? reader = null;

            try
            {
                items.ForEach(item =>
                {
                    item.Version++;
                    item.Deleted = true;
                    item.LastUser = lastUser;
                    item.LastTime = TimeUtil.UtcNow;
                });

                dbCommand = _sqlBuilder.CreateBatchDeleteCommand(entityDef, items);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext.Transaction,
                    entityDef.DatabaseName!,
                    dbCommand,
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
            finally
            {
                reader?.Dispose();
                dbCommand?.Dispose();
            }
        }

        #endregion

        private static void ThrowIfDatabaseInitLockNotGet(IEnumerable<string> databaseNames)
        {
            throw new DatabaseException(ErrorCode.DatabaseInitLockError, $"Database:{databaseNames.ToJoinedString(",")}");
        }
    }
}
