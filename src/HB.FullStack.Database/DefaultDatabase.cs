

using HB.FullStack.Common;
using HB.FullStack.Common.Extensions;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;

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
    public sealed class DefaultDatabase : IDatabase
    {
        private readonly DatabaseCommonSettings _databaseSettings;
        private readonly IDatabaseEngine _databaseEngine;
        private readonly ITransaction _transaction;
        private readonly ILogger _logger;
        private readonly string _deletedPropertyReservedName;
        private bool _initialized;

        public IEntityDefFactory EntityDefFactory { get; }
        public IDbCommandBuilder DbCommandBuilder { get; }

        public EngineType EngineType { get; }
        IDatabaseEngine IDatabase.DatabaseEngine => _databaseEngine;
        public IEnumerable<string> DatabaseNames => _databaseEngine.DatabaseNames;
        public int VarcharDefaultLength { get; }

        public DefaultDatabase(
            IDatabaseEngine databaseEngine,
            IEntityDefFactory entityDefFactory,
            IDbCommandBuilder commandBuilder,
            ITransaction transaction,
            ILogger<DefaultDatabase> logger)
        {
            if (databaseEngine.DatabaseSettings.Version < 0)
            {
                throw DatabaseExceptions.VersionShouldBePositive(databaseEngine.DatabaseSettings.Version);
            }

            _databaseSettings = databaseEngine.DatabaseSettings;
            _databaseEngine = databaseEngine;
            EntityDefFactory = entityDefFactory;
            DbCommandBuilder = commandBuilder;
            _transaction = transaction;
            _logger = logger;

            EngineType = databaseEngine.EngineType;


            VarcharDefaultLength = _databaseSettings.DefaultVarcharLength == 0 ? DefaultLengthConventions.DEFAULT_VARCHAR_LENGTH : _databaseSettings.DefaultVarcharLength;


            _deletedPropertyReservedName = SqlHelper.GetReserved(nameof(Entity.Deleted), _databaseEngine.EngineType);
        }

        #region Initialize

        /// <summary>
        /// 初始化，如果在服务端，请加全局分布式锁来初始化
        /// </summary>
        public async Task InitializeAsync(IEnumerable<Migration>? migrations = null)
        {
            using IDisposable? scope = _logger.BeginScope("数据库初始化");

            if (_databaseSettings.AutomaticCreateTable)
            {
                IEnumerable<Migration>? initializeMigrations = migrations?.Where(m => m.OldVersion == 0 && m.NewVersion == 1);

                await AutoCreateTablesIfBrandNewAsync(_databaseSettings.AddDropStatementWhenCreateTable, initializeMigrations).ConfigureAwait(false);

                _logger.LogInformation("Database Auto Create Tables Finished.");
            }

            if (migrations != null && migrations.Any())
            {
                await MigarateAsync(migrations).ConfigureAwait(false);

                _logger.LogInformation("Database Migarate Finished.");
            }

            _initialized = true;

            _logger.LogInformation("数据初始化成功！");
        }

        private async Task AutoCreateTablesIfBrandNewAsync(bool addDropStatement, IEnumerable<Migration>? initializeMigrations)
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
                        //要求新数据必须从version = 1开始
                        if (_databaseSettings.Version != 1)
                        {
                            await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                            throw DatabaseExceptions.TableCreateError(_databaseSettings.Version, databaseName, "Database does not exists, database Version must be 1");
                        }

                        await CreateTablesByDatabaseAsync(databaseName, transactionContext, addDropStatement).ConfigureAwait(false);

                        await UpdateSystemVersionAsync(databaseName, 1, transactionContext.Transaction).ConfigureAwait(false);

                        //初始化数据
                        IEnumerable<Migration>? curInitMigrations = initializeMigrations?.Where(m => m.DatabaseName.Equals(databaseName, GlobalSettings.ComparisonIgnoreCase)).ToList();

                        if (curInitMigrations.IsNotNullOrEmpty())
                        {
                            if (curInitMigrations.Count() > 1)
                            {
                                throw DatabaseExceptions.MigrateError(databaseName, "Database have more than one Initialize Migrations");
                            }

                            await ApplyMigration(databaseName, transactionContext, curInitMigrations.First()).ConfigureAwait(false);
                        }
                    }

                    await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
                }
                catch (DatabaseException)
                {
                    await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                    throw;
                }
                catch (Exception ex)
                {
                    await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                    throw DatabaseExceptions.TableCreateError(_databaseSettings.Version, databaseName, "Unkown", ex);
                }
            }
        }

        private Task<int> CreateTableAsync(EntityDef def, TransactionContext transContext, bool addDropStatement)
        {
            var command = DbCommandBuilder.CreateTableCreateCommand(EngineType, def, addDropStatement, VarcharDefaultLength);

            _logger.LogInformation("Table创建：{CommandText}", command.CommandText);

            return _databaseEngine.ExecuteCommandNonQueryAsync(transContext.Transaction, def.DatabaseName!, command);
        }

        private async Task CreateTablesByDatabaseAsync(string databaseName, TransactionContext transactionContext, bool addDropStatement)
        {
            foreach (EntityDef entityDef in EntityDefFactory.GetAllDefsByDatabase(databaseName))
            {
                await CreateTableAsync(entityDef, transactionContext, addDropStatement).ConfigureAwait(false);
            }
        }

        private async Task MigarateAsync(IEnumerable<Migration> migrations)
        {
            if (migrations != null && migrations.Any(m => m.NewVersion <= m.OldVersion))
            {
                throw DatabaseExceptions.MigrateError("", "Migraion NewVersion <= OldVersion");
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
                            throw DatabaseExceptions.MigrateError(sys.DatabaseName, "Lack Migrations");
                        }

                        IEnumerable<Migration> curOrderedMigrations = migrations
                            .Where(m => m.DatabaseName.Equals(sys.DatabaseName, GlobalSettings.ComparisonIgnoreCase))
                            .OrderBy(m => m.OldVersion).ToList();

                        if (curOrderedMigrations == null)
                        {
                            throw DatabaseExceptions.MigrateError(sys.DatabaseName, "Lack Migrations");
                        }

                        if (!CheckMigrations(sys.Version, _databaseSettings.Version, curOrderedMigrations!))
                        {
                            throw DatabaseExceptions.MigrateError(sys.DatabaseName, "Migrations not sufficient.");
                        }

                        foreach (Migration migration in curOrderedMigrations!)
                        {
                            await ApplyMigration(databaseName, transactionContext, migration).ConfigureAwait(false);
                        }

                        await UpdateSystemVersionAsync(sys.DatabaseName, _databaseSettings.Version, transactionContext.Transaction).ConfigureAwait(false);
                    }

                    await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
                }
                catch (DatabaseException)
                {
                    await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                    throw;
                }
                catch (Exception ex)
                {
                    await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                    throw DatabaseExceptions.MigrateError(databaseName, "", ex);
                }
            }
        }

        private async Task ApplyMigration(string databaseName, TransactionContext transactionContext, Migration migration)
        {
            _logger.LogInformation(
                "数据库Migration, {DatabaseName}, from {OldVersion}, to {NewVersion}, {Sql}",
                databaseName, migration.OldVersion, migration.NewVersion, migration.SqlStatement);

            if (migration.SqlStatement.IsNotNullOrEmpty())
            {
                EngineCommand command = new EngineCommand(migration.SqlStatement);

                await _databaseEngine.ExecuteCommandNonQueryAsync(transactionContext.Transaction, databaseName, command).ConfigureAwait(false);
            }

            if (migration.ModifyFunc != null)
            {
                await migration.ModifyFunc(this, transactionContext).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 检查是否依次提供了不中断的Migration
        /// </summary>
        /// <param name="startVersion"></param>
        /// <param name="endVersion"></param>
        /// <param name="curOrderedMigrations"></param>
        /// <returns></returns>
        private static bool CheckMigrations(int startVersion, int endVersion, IEnumerable<Migration> curOrderedMigrations)
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

            object? result = await _databaseEngine.ExecuteCommandScalarAsync(transaction, databaseName, command, true).ConfigureAwait(false);

            return Convert.ToBoolean(result, GlobalSettings.Culture);
        }

        internal async Task<SystemInfo> GetSystemInfoAsync(string databaseName, IDbTransaction transaction)
        {
            bool isExisted = await IsTableExistsAsync(databaseName, SystemInfoNames.SYSTEM_INFO_TABLE_NAME, transaction).ConfigureAwait(false);

            if (!isExisted)
            {
                return new SystemInfo(databaseName) { Version = 0 };
            }

            var command = DbCommandBuilder.CreateSystemInfoRetrieveCommand(EngineType);

            using IDataReader reader = await _databaseEngine.ExecuteCommandReaderAsync(transaction, databaseName, command, false).ConfigureAwait(false);

            SystemInfo systemInfo = new SystemInfo(databaseName);

            while (reader.Read())
            {
                systemInfo.Set(reader["Name"].ToString()!, reader["Value"].ToString()!);
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

        public FromExpression<T> From<T>() where T : DatabaseEntity, new() => DbCommandBuilder.From<T>(EngineType);

        public WhereExpression<T> Where<T>() where T : DatabaseEntity, new() => DbCommandBuilder.Where<T>(EngineType);

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DatabaseEntity, new() => DbCommandBuilder.Where<T>(EngineType, sqlFilter, filterParams);

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DatabaseEntity, new() => DbCommandBuilder.Where(EngineType, predicate);

        #endregion

        #region 单表查询 From, Where

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
                throw DatabaseExceptions.FoundTooMuch(type: typeof(T).FullName, from: fromCondition?.ToStatement(), where: whereCondition?.ToStatement(_databaseEngine.EngineType));
            }

            return lst.ElementAt(0);
        }

        public async Task<IEnumerable<TSelect>> RetrieveAsync<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, TransactionContext? transContext = null)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            if (whereCondition == null)
            {
                whereCondition = Where<TWhere>();
            }

            EntityDef selectDef = EntityDefFactory.GetDef<TSelect>()!;
            EntityDef fromDef = EntityDefFactory.GetDef<TFrom>()!;
            EntityDef whereDef = EntityDefFactory.GetDef<TWhere>()!;

            whereCondition.And($"{whereDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {selectDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {fromDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType, fromCondition, whereCondition, selectDef);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, selectDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);

                return reader.ToEntities<TSelect>(_databaseEngine.EngineType, EntityDefFactory, selectDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: selectDef.EntityFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            whereCondition.And($"{entityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand(EngineType, entityDef, fromCondition, whereCondition);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToEntities<T>(_databaseEngine.EngineType, EntityDefFactory, entityDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: entityDef.EntityFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            whereCondition.And($"{entityDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                var command = DbCommandBuilder.CreateCountCommand(EngineType, fromCondition, whereCondition);
                object? countObj = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return Convert.ToInt32(countObj, GlobalSettings.Culture);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: entityDef.EntityFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        #endregion

        #region 单表查询, Where

        public Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext, int? page, int? perPage, string? orderBy)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> where = Where<T>().AddOrderAndLimits(page, perPage, orderBy);

            return RetrieveAsync<T>(null, where, transContext);
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
            where T : LongIdEntity, new()
        {
            WhereExpression<T> where = Where<T>($"{SqlHelper.GetReserved(nameof(LongIdEntity.Id), EngineType)}={{0}}", id);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(Guid id, TransactionContext? transContext)
            where T : GuidEntity, new()
        {
            //WhereExpression<T> where = Where<T>($"{SqlHelper.GetReserved(nameof(GuidEntity.Id), EngineType)}={{0}}", guid);
            WhereExpression<T> where = Where<T>(t => t.Id == id);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext, int? page, int? perPage, string? orderBy)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr).AddOrderAndLimits(page, perPage, orderBy);

            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return CountAsync(null, whereCondition, transContext);
        }

        //TODO: orderby 添加对 desc的支持
        /// <summary>
        /// 根据给出的外键值获取 page从0开始
        /// </summary>
        public async Task<IEnumerable<T>> RetrieveByForeignKeyAsync<T>(Expression<Func<T, object>> foreignKeyExp, object foreignKeyValue, TransactionContext? transactionContext, int? page, int? perPage, string? orderBy)
            where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            string foreignKeyName = ((MemberExpression)foreignKeyExp.Body).Member.Name;

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            EntityPropertyDef? foreignKeyProperty = entityDef.GetPropertyDef(foreignKeyName);

            if (foreignKeyProperty == null || !foreignKeyProperty.IsForeignKey)
            {
                throw DatabaseExceptions.NoSuchForeignKey(entityDef.EntityFullName, foreignKeyName);
            }

            Type foreignKeyValueType = foreignKeyValue.GetType();

            if (foreignKeyValueType != typeof(long) && foreignKeyValueType != typeof(Guid))
            {
                throw DatabaseExceptions.KeyValueNotLongOrGuid(entityDef.EntityFullName, foreignKeyName, foreignKeyValue, foreignKeyValueType.FullName);
            }

            WhereExpression<T> where = Where<T>($"{foreignKeyName}={{0}}", foreignKeyValue)
                .AddOrderAndLimits(page, perPage, orderBy);

            return await RetrieveAsync(where, transactionContext).ConfigureAwait(false);
        }

        #endregion

        #region 双表查询

        public async Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
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
                return reader.ToEntities<TSource, TTarget>(_databaseEngine.EngineType, EntityDefFactory, sourceEntityDef, targetEntityDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: sourceEntityDef.EntityFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            IEnumerable<Tuple<TSource, TTarget?>> lst = await RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext).ConfigureAwait(false);

            if (lst.IsNullOrEmpty())
            {
                return null;
            }

            if (lst.Count() > 1)
            {
                throw DatabaseExceptions.FoundTooMuch(typeof(TSource).FullName, from: fromCondition?.ToStatement(), where: whereCondition?.ToStatement(_databaseEngine.EngineType));
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
            ThrowIfNotInitializedYet();

            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
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
                return reader.ToEntities<TSource, TTarget1, TTarget2>(_databaseEngine.EngineType, EntityDefFactory, sourceEntityDef, targetEntityDef1, targetEntityDef2);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: sourceEntityDef.EntityFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>> lst = await RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext).ConfigureAwait(false);

            if (lst.IsNullOrEmpty())
            {
                return null;
            }

            if (lst.Count() > 1)
            {
                throw DatabaseExceptions.FoundTooMuch(typeof(TSource).FullName, fromCondition.ToStatement(), whereCondition?.ToStatement(_databaseEngine.EngineType));
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
            ThrowIfNotInitializedYet();

            ThrowIf.NotValid(item, nameof(item));

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(entityDef);

            TruncateLastUser(ref lastUser, item, entityDef);

            try
            {
                PrepareItem(item, lastUser);

                var command = DbCommandBuilder.CreateAddCommand(EngineType, entityDef, item);

                object? rt = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName!, command, true).ConfigureAwait(false);

                if (entityDef.IsIdAutoIncrement)
                {
                    ((AutoIncrementIdEntity)(object)item).Id = Convert.ToInt64(rt, CultureInfo.InvariantCulture);
                }
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItem(item);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item);
                }

                throw DatabaseExceptions.UnKown(type: entityDef.EntityFullName, item: SerializeUtil.ToJson(item), ex);
            }

            static void PrepareItem(T item, string lastUser)
            {
                DateTimeOffset utcNow = TimeUtil.UtcNow;
                item.Version = 0;
                item.LastUser = lastUser;
                item.LastTime = utcNow;
                item.CreateTime = utcNow;
            }

            static void RestoreItem(T item)
            {
                item.Version = -1;
            }
        }

        /// <summary>
        /// Version控制,反应Version变化
        /// </summary>
        public async Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            ThrowIf.NotValid(item, nameof(item));

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(entityDef);

            TruncateLastUser(ref lastUser, item, entityDef);

            try
            {
                PrepareItem(item, lastUser);

                var command = DbCommandBuilder.CreateDeleteCommand(EngineType, entityDef, item);

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DatabaseExceptions.NotFound(type: entityDef.EntityFullName, item: SerializeUtil.ToJson(item), "");
                }
                else
                {
                    throw DatabaseExceptions.FoundTooMuch(entityDef.EntityFullName, item: SerializeUtil.ToJson(item));
                }
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItem(item);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item);
                }

                throw DatabaseExceptions.UnKown(entityDef.EntityFullName, SerializeUtil.ToJson(item), ex);
            }

            static void PrepareItem(T item, string lastUser)
            {
                item.Deleted = true;
                item.Version++;
                item.LastUser = lastUser;
                item.LastTime = TimeUtil.UtcNow;
            }

            static void RestoreItem(T item)
            {
                item.Deleted = false;
                item.Version--;
            }
        }

        /// <summary>
        ///  修改，建议每次修改前先select，并放置在一个事务中。
        ///  版本控制，如果item中Version未赋值，会无法更改
        ///  反应Version变化
        /// </summary>
        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            ThrowIf.NotValid(item, nameof(item));

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(entityDef);

            TruncateLastUser(ref lastUser, item, entityDef);

            try
            {
                PrepareItem(item, lastUser);

                EngineCommand command = DbCommandBuilder.CreateUpdateCommand(EngineType, entityDef, item);
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DatabaseExceptions.NotFound(entityDef.EntityFullName, SerializeUtil.ToJson(item), "");
                }

                throw DatabaseExceptions.FoundTooMuch(entityDef.EntityFullName, SerializeUtil.ToJson(item));
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItem(item);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item);
                }

                throw DatabaseExceptions.UnKown(entityDef.EntityFullName, SerializeUtil.ToJson(item), ex);
            }

            static void PrepareItem(T item, string lastUser)
            {
                item.LastUser = lastUser;
                item.LastTime = TimeUtil.UtcNow;
                item.Version++;
            }

            static void RestoreItem(T item)
            {
                item.Version--;
            }
        }

        public async Task UpdateFieldsAsync<T>(object id, int version, string lastUser, IDictionary<string, object?> propertyValues, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            if (id is long longId && longId <= 0)
            {
                throw DatabaseExceptions.LongIdShouldBePositive(longId);
            }

            if (id is Guid guid && guid.IsEmpty())
            {
                throw DatabaseExceptions.GuidShouldNotEmpty();
            }

            if (version < 0)
            {
                throw DatabaseExceptions.VersionShouldBePositive(version);
            }

            if (propertyValues.Count <= 0)
            {
                throw DatabaseExceptions.UpdatePropertiesCountShouldBePositive();
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(entityDef);

            TruncateLastUser(ref lastUser, id);

            try
            {
                EngineCommand command = DbCommandBuilder.CreateUpdateFieldsCommand(EngineType, entityDef, id, version + 1, lastUser, propertyValues);
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DatabaseExceptions.NotFound(entityDef.EntityFullName, $"id:{id}, version:{version}, propertyValues:{SerializeUtil.ToJson(propertyValues)}", "");
                }

                throw DatabaseExceptions.FoundTooMuch(entityDef.EntityFullName, $"id:{id}, version:{version}, propertyValues:{SerializeUtil.ToJson(propertyValues)}");
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(entityDef.EntityFullName, $"id:{id}, version:{version}, propertyValues:{SerializeUtil.ToJson(propertyValues)}", ex);
            }
        }

        #endregion

        #region 批量更改(Write)

        /// <summary>
        /// BatchAddAsync，反应Version变化
        /// </summary>
        public async Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            if (_databaseEngine.DatabaseSettings.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchAdd超过批量操作的最大数目", items.Count(), lastUser);
            }

            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return new List<object>();
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(entityDef);

            TruncateLastUser(ref lastUser, items, entityDef);

            try
            {
                PrepareItems(items, lastUser);

                IList<object> newIds = new List<object>();

                var command = DbCommandBuilder.CreateBatchAddCommand(EngineType, entityDef, items, transContext == null);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext?.Transaction,
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
                        ((AutoIncrementIdEntity)(object)item).Id = Convert.ToInt64(newIds[num++], GlobalSettings.Culture);
                    }
                }
                else if (entityDef.IsIdGuid)
                {
                    foreach (var item in items)
                    {
                        newIds.Add(((GuidEntity)(object)item).Id);
                    }
                }
                else if (entityDef.IsIdLong)
                {
                    foreach (var item in items)
                    {
                        newIds.Add(((LongIdEntity)(object)item).Id);
                    }
                }

                return newIds;
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItems(items);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItems(items);
                }

                throw DatabaseExceptions.UnKown(entityDef.EntityFullName, SerializeUtil.ToJson(items), ex);
            }

            static void PrepareItems(IEnumerable<T> items, string lastUser)
            {
                DateTimeOffset utcNow = TimeUtil.UtcNow;

                foreach (var item in items)
                {
                    item.Version = 0;
                    item.LastUser = lastUser;
                    item.LastTime = utcNow;
                    item.CreateTime = utcNow;
                }
            }

            static void RestoreItems(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    item.Version = -1;
                }
            }
        }

        /// <summary>
        /// 批量更改，反应Version变化
        /// </summary>
        public async Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            if (_databaseEngine.DatabaseSettings.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchUpdate超过批量操作的最大数目", items.Count(), lastUser);
            }

            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(entityDef);

            TruncateLastUser(ref lastUser, items, entityDef);

            try
            {
                PrepareItems(items, lastUser);

                var command = DbCommandBuilder.CreateBatchUpdateCommand(EngineType, entityDef, items, transContext == null);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext?.Transaction,
                    entityDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DatabaseExceptions.NotFound(entityDef.EntityFullName, SerializeUtil.ToJson(items), "BatchUpdate");
                    }

                    count++;
                }

                if (count != items.Count())
                {
                    throw DatabaseExceptions.NotFound(entityDef.EntityFullName, SerializeUtil.ToJson(items), "");
                }
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItems(items);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItems(items);
                }

                throw DatabaseExceptions.UnKown(entityDef.EntityFullName, SerializeUtil.ToJson(items), ex);
            }

            static void PrepareItems(IEnumerable<T> items, string lastUser)
            {
                foreach (var item in items)
                {
                    item.Version++;
                    item.LastUser = lastUser;
                    item.LastTime = TimeUtil.UtcNow;
                }
            }

            static void RestoreItems(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    item.Version--;
                }
            }
        }

        /// <summary>
        /// BatchDeleteAsync, 反应version的变化
        /// </summary>
        public async Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new()
        {
            ThrowIfNotInitializedYet();

            if (_databaseEngine.DatabaseSettings.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchDelete超过批量操作的最大数目", items.Count(), lastUser);
            }

            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(entityDef);

            TruncateLastUser(ref lastUser, items, entityDef);

            try
            {
                PrepareItems(items, lastUser);

                var command = DbCommandBuilder.CreateBatchDeleteCommand(EngineType, entityDef, items, transContext == null);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext?.Transaction,
                    entityDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int affected = reader.GetInt32(0);

                    if (affected != 1)
                    {
                        throw DatabaseExceptions.NotFound(entityDef.EntityFullName, SerializeUtil.ToJson(items), $"not found the {count}th data item");
                    }

                    count++;
                }

                if (count != items.Count())
                {
                    throw DatabaseExceptions.NotFound(entityDef.EntityFullName, SerializeUtil.ToJson(items), "");
                }
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItems(items);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItems(items);
                }

                throw DatabaseExceptions.UnKown(entityDef.EntityFullName, SerializeUtil.ToJson(items), ex);
            }

            static void PrepareItems(IEnumerable<T> items, string lastUser)
            {
                foreach (var item in items)
                {
                    item.Version++;
                    item.Deleted = true;
                    item.LastUser = lastUser;
                    item.LastTime = TimeUtil.UtcNow;
                }
            }

            static void RestoreItems(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    item.Version--;
                    item.Deleted = false;
                }
            }
        }

        #endregion

        private void ThrowIfNotInitializedYet()
        {
            if (!_initialized)
            {
                throw DatabaseExceptions.NotInitializedYet();
            }
        }

        private static void ThrowIfNotWriteable(EntityDef entityDef)
        {
            if (!entityDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(type: entityDef.EntityFullName, database: entityDef.DatabaseName);
            }
        }

        private void TruncateLastUser<T>(ref string lastUser, T item, EntityDef entityDef) where T : DatabaseEntity, new()
        {
            if (lastUser.Length > DefaultLengthConventions.MAX_LAST_USER_LENGTH)
            {
                object id = entityDef.IsIdLong ? ((LongIdEntity)(object)item).Id : entityDef.IsIdGuid ? ((GuidEntity)(object)item).Id : "None";
                _logger.LogWarning("LastUser 截断. {LastUser}, {Id}", lastUser, id);

                lastUser = lastUser.Substring(0, DefaultLengthConventions.MAX_LAST_USER_LENGTH);
            }
        }

        private void TruncateLastUser(ref string lastUser, object id)
        {
            if (lastUser.Length > DefaultLengthConventions.MAX_LAST_USER_LENGTH)
            {
                _logger.LogWarning("LastUser 截断. {LastUser}, {Id}", lastUser, id);

                lastUser = lastUser.Substring(0, DefaultLengthConventions.MAX_LAST_USER_LENGTH);
            }
        }

        private void TruncateLastUser<T>(ref string lastUser, IEnumerable<T> items, EntityDef entityDef) where T : DatabaseEntity, new()
        {
            foreach (T item in items)
            {
                TruncateLastUser(ref lastUser, item, entityDef);
            }
        }
    }
}