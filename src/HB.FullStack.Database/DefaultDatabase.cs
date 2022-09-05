
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Extensions;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

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
    public sealed partial class DefaultDatabase : IDatabase
    {
        private readonly DatabaseCommonSettings _databaseSettings;
        private readonly IDatabaseEngine _databaseEngine;
        private readonly ITransaction _transaction;
        private readonly ILogger _logger;
        private readonly string _deletedPropertyReservedName;

        public IDbModelDefFactory ModelDefFactory { get; }
        public IDbCommandBuilder DbCommandBuilder { get; }

        public EngineType EngineType { get; }
        IDatabaseEngine IDatabase.DatabaseEngine => _databaseEngine;
        public IEnumerable<string> DatabaseNames => _databaseEngine.DatabaseNames;
        public int VarcharDefaultLength { get; }

        public DefaultDatabase(
            IDatabaseEngine databaseEngine,
            IDbModelDefFactory modelDefFactory,
            IDbCommandBuilder commandBuilder,
            ITransaction transaction,
            ILogger<DefaultDatabase> logger)
        {
            if (databaseEngine.DatabaseSettings.Version < 0)
            {
                throw DatabaseExceptions.TimestampShouldBePositive(databaseEngine.DatabaseSettings.Version);
            }

            _databaseSettings = databaseEngine.DatabaseSettings;
            _databaseEngine = databaseEngine;
            ModelDefFactory = modelDefFactory;
            DbCommandBuilder = commandBuilder;
            _transaction = transaction;
            _logger = logger;

            EngineType = databaseEngine.EngineType;

            VarcharDefaultLength = _databaseSettings.DefaultVarcharLength == 0 ? DefaultLengthConventions.DEFAULT_VARCHAR_LENGTH : _databaseSettings.DefaultVarcharLength;

            _deletedPropertyReservedName = SqlHelper.GetReserved(nameof(DbModel.Deleted), _databaseEngine.EngineType);
        }

        #region Initialize

        /// <summary>
        /// 初始化，如果在服务端，请加全局分布式锁来初始化
        /// 返回是否真正执行了Migration
        /// </summary>
        public async Task<bool> InitializeAsync(IEnumerable<Migration>? migrations = null)
        {
            using IDisposable? scope = _logger.BeginScope("数据库初始化");

            if (_databaseSettings.AutomaticCreateTable)
            {
                IEnumerable<Migration>? initializeMigrations = migrations?.Where(m => m.OldVersion == 0 && m.NewVersion == 1);

                await AutoCreateTablesIfBrandNewAsync(_databaseSettings.AddDropStatementWhenCreateTable, initializeMigrations).ConfigureAwait(false);

                _logger.LogInformation("Database Auto Create Tables Finished.");
            }

            bool migrationExecuted = false;

            if (migrations != null && migrations.Any())
            {
                migrationExecuted = await MigarateAsync(migrations).ConfigureAwait(false);

                _logger.LogInformation("Database Migarate Finished.");
            }

            _logger.LogInformation("数据初始化成功！");

            return migrationExecuted;
        }

        private async Task AutoCreateTablesIfBrandNewAsync(bool addDropStatement, IEnumerable<Migration>? initializeMigrations)
        {
            foreach (string databaseName in _databaseEngine.DatabaseNames)
            {
                TransactionContext transactionContext = await _transaction.BeginTransactionAsync(databaseName, IsolationLevel.Serializable).ConfigureAwait(false);

                try
                {
                    SystemInfo? sys = await GetSystemInfoAsync(databaseName, transactionContext.Transaction).ConfigureAwait(false);

                    //表明是新数据库
                    if (sys == null)
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

        private Task<int> CreateTableAsync(DbModelDef def, TransactionContext transContext, bool addDropStatement)
        {
            var command = DbCommandBuilder.CreateTableCreateCommand(EngineType, def, addDropStatement, VarcharDefaultLength);

            _logger.LogInformation("Table创建：{CommandText}", command.CommandText);

            return _databaseEngine.ExecuteCommandNonQueryAsync(transContext.Transaction, def.DatabaseName!, command);
        }

        private async Task CreateTablesByDatabaseAsync(string databaseName, TransactionContext transactionContext, bool addDropStatement)
        {
            foreach (DbModelDef modelDef in ModelDefFactory.GetAllDefsByDatabase(databaseName))
            {
                await CreateTableAsync(modelDef, transactionContext, addDropStatement).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 返回是否真正执行过Migration
        /// </summary>
        private async Task<bool> MigarateAsync(IEnumerable<Migration> migrations)
        {
            if (migrations != null && migrations.Any(m => m.NewVersion <= m.OldVersion))
            {
                throw DatabaseExceptions.MigrateError("", "Migraion NewVersion <= OldVersion");
            }

            bool migrationExecuted = false;

            foreach (string databaseName in _databaseEngine.DatabaseNames)
            {
                TransactionContext transactionContext = await _transaction.BeginTransactionAsync(databaseName, IsolationLevel.Serializable).ConfigureAwait(false);

                try
                {

                    SystemInfo? sys = await GetSystemInfoAsync(databaseName, transactionContext.Transaction).ConfigureAwait(false);

                    if (sys!.Version < _databaseSettings.Version)
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
                            migrationExecuted = true;
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

            return migrationExecuted;
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

        internal async Task<SystemInfo?> GetSystemInfoAsync(string databaseName, IDbTransaction transaction)
        {
            bool isExisted = await IsTableExistsAsync(databaseName, SystemInfoNames.SYSTEM_INFO_TABLE_NAME, transaction).ConfigureAwait(false);

            if (!isExisted)
            {
                //return new SystemInfo(databaseName) { Version = 0 };
                return null;
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

        public FromExpression<T> From<T>() where T : DbModel, new() => DbCommandBuilder.From<T>(EngineType);

        public WhereExpression<T> Where<T>() where T : DbModel, new() => DbCommandBuilder.Where<T>(EngineType);

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DbModel, new() => DbCommandBuilder.Where<T>(EngineType, sqlFilter, filterParams);

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DbModel, new() => DbCommandBuilder.Where(EngineType, predicate);

        #endregion

        #region 单表查询 From, Where

        public async Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
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
            where TSelect : DbModel, new()
            where TFrom : DbModel, new()
            where TWhere : DbModel, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TWhere>();
            }

            DbModelDef selectDef = ModelDefFactory.GetDef<TSelect>()!;
            DbModelDef fromDef = ModelDefFactory.GetDef<TFrom>()!;
            DbModelDef whereDef = ModelDefFactory.GetDef<TWhere>()!;

            whereCondition.And($"{whereDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {selectDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {fromDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                EngineCommand command = DbCommandBuilder.CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType, fromCondition, whereCondition, selectDef);

                using IDataReader reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, selectDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);

                return reader.ToModels<TSelect>(_databaseEngine.EngineType, ModelDefFactory, selectDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: selectDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            whereCondition.And($"{modelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                EngineCommand command = DbCommandBuilder.CreateRetrieveCommand(EngineType, modelDef, fromCondition, whereCondition);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, modelDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToModels<T>(_databaseEngine.EngineType, ModelDefFactory, modelDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: modelDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            whereCondition.And($"{modelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                EngineCommand command = DbCommandBuilder.CreateCountCommand(EngineType, fromCondition, whereCondition);
                object? countObj = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, modelDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return Convert.ToInt32(countObj, GlobalSettings.Culture);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: modelDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        #endregion

        #region 单表查询, Where

        public Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext, int? page, int? perPage, string? orderBy)
            where T : DbModel, new()
        {
            WhereExpression<T> where = Where<T>().AddOrderAndLimits(page, perPage, orderBy);

            return RetrieveAsync(null, where, transContext);
        }

        public Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            return CountAsync(null, condition, transContext);
        }

        public Task<long> CountAsync<T>(TransactionContext? transContext)
            where T : DbModel, new()
        {
            return CountAsync<T>(null, null, transContext);
        }

        #endregion

        #region 单表查询, Expression Where

        public Task<T?> ScalarAsync<T>(long id, TransactionContext? transContext)
            where T : DbModel, ILongId, new()
        {
            WhereExpression<T> where = Where<T>($"{SqlHelper.GetReserved(nameof(ILongId.Id), EngineType)}={{0}}", id);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(Guid id, TransactionContext? transContext)
            where T : DbModel, IGuidId, new()
        {
            //WhereExpression<T> where = Where<T>($"{SqlHelper.GetReserved(nameof(TimestampGuidDbModel.Id), EngineType)}={{0}}", guid);
            WhereExpression<T> where = Where<T>(t => t.Id == id);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DbModel, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext, int? page, int? perPage, string? orderBy)
            where T : DbModel, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr).AddOrderAndLimits(page, perPage, orderBy);

            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext)
            where T : DbModel, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return CountAsync(null, whereCondition, transContext);
        }

        //TODO: orderby 添加对 desc的支持
        /// <summary>
        /// 根据给出的外键值获取 page从0开始
        /// </summary>
        public async Task<IEnumerable<T>> RetrieveByForeignKeyAsync<T>(Expression<Func<T, object>> foreignKeyExp, object foreignKeyValue, TransactionContext? transactionContext, int? page, int? perPage, string? orderBy)
            where T : DbModel, new()
        {
            string foreignKeyName = ((MemberExpression)foreignKeyExp.Body).Member.Name;

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            DbModelPropertyDef? foreignKeyProperty = modelDef.GetPropertyDef(foreignKeyName);

            if (foreignKeyProperty == null || !foreignKeyProperty.IsForeignKey)
            {
                throw DatabaseExceptions.NoSuchForeignKey(modelDef.ModelFullName, foreignKeyName);
            }

            Type foreignKeyValueType = foreignKeyValue.GetType();

            if (foreignKeyValueType != typeof(long) && foreignKeyValueType != typeof(Guid))
            {
                throw DatabaseExceptions.KeyValueNotLongOrGuid(modelDef.ModelFullName, foreignKeyName, foreignKeyValue, foreignKeyValueType.FullName);
            }

            WhereExpression<T> where = Where<T>($"{foreignKeyName}={{0}}", foreignKeyValue)
                .AddOrderAndLimits(page, perPage, orderBy);

            return await RetrieveAsync(where, transactionContext).ConfigureAwait(false);
        }

        #endregion

        #region 双表查询

        public async Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DbModel, new()
            where TTarget : DbModel, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            DbModelDef sourceModelDef = ModelDefFactory.GetDef<TSource>()!;
            DbModelDef targetModelDef = ModelDefFactory.GetDef<TTarget>()!;

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false);
                    break;

                case SqlJoinType.RIGHT:
                    whereCondition.And($"{targetModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And<TTarget>(t => t.Deleted == false);
                    break;

                case SqlJoinType.INNER:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;

                case SqlJoinType.FULL:
                    break;

                case SqlJoinType.CROSS:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;
            }

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand<TSource, TTarget>(EngineType, fromCondition, whereCondition, sourceModelDef, targetModelDef);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, sourceModelDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToModels<TSource, TTarget>(_databaseEngine.EngineType, ModelDefFactory, sourceModelDef, targetModelDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: sourceModelDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DbModel, new()
            where TTarget : DbModel, new()
        {
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
            where TSource : DbModel, new()
            where TTarget1 : DbModel, new()
            where TTarget2 : DbModel, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            DbModelDef sourceModelDef = ModelDefFactory.GetDef<TSource>()!;
            DbModelDef targetModelDef1 = ModelDefFactory.GetDef<TTarget1>()!;
            DbModelDef targetModelDef2 = ModelDefFactory.GetDef<TTarget2>()!;

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false);
                    break;

                case SqlJoinType.RIGHT:
                    whereCondition.And($"{targetModelDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And<TTarget2>(t => t.Deleted == false);
                    break;

                case SqlJoinType.INNER:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef1.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;

                case SqlJoinType.FULL:
                    break;

                case SqlJoinType.CROSS:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef1.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;
            }

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(EngineType, fromCondition, whereCondition, sourceModelDef, targetModelDef1, targetModelDef2);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, sourceModelDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToModels<TSource, TTarget1, TTarget2>(_databaseEngine.EngineType, ModelDefFactory, sourceModelDef, targetModelDef1, targetModelDef2);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: sourceModelDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DbModel, new()
            where TTarget1 : DbModel, new()
            where TTarget2 : DbModel, new()
        {
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
        public async Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, item, modelDef);

            long oldTimestamp = -1;
            string oldLastUser = "";

            try
            {
                //Prepare
                if (item is TimestampDbModel serverModel)
                {
                    oldTimestamp = serverModel.Timestamp;
                    oldLastUser = serverModel.LastUser;

                    serverModel.Timestamp = TimeUtil.UtcNowTicks;
                    serverModel.LastUser = lastUser;
                }

                var command = DbCommandBuilder.CreateAddCommand(EngineType, modelDef, item);

                object? rt = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, modelDef.DatabaseName!, command, true).ConfigureAwait(false);

                if (modelDef.IsIdAutoIncrement)
                {
                    ((ILongId)item).Id = Convert.ToInt64(rt, CultureInfo.InvariantCulture);
                }
            }
            catch (DatabaseException ex)
            {
                //TODO:捕捉主键冲突而无法添加，即重复请求了

                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw DatabaseExceptions.UnKown(type: modelDef.ModelFullName, item: SerializeUtil.ToJson(item), ex);
            }

            static void RestoreItem(T item, long oldTimestamp, string oldLastUser)
            {
                if (item is TimestampDbModel serverModel)
                {
                    serverModel.Timestamp = oldTimestamp;
                    serverModel.LastUser = oldLastUser;
                }
            }
        }

        /// <summary>
        /// Version控制,反应Version变化
        /// </summary>
        public async Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, item, modelDef);

            long oldTimestamp = -1;
            string oldLastUser = "";

            try
            {
                if (item is TimestampDbModel timestampDBModel)
                {
                    oldTimestamp = timestampDBModel.Timestamp;
                    oldLastUser = timestampDBModel.LastUser;

                    timestampDBModel.Deleted = true;
                    timestampDBModel.Timestamp = TimeUtil.UtcNowTicks;
                    timestampDBModel.LastUser = lastUser;
                }

                var command = DbCommandBuilder.CreateDeleteCommand(EngineType, modelDef, item, oldTimestamp);

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(type: modelDef.ModelFullName, item: SerializeUtil.ToJson(item), "");
                }
                else
                {
                    throw DatabaseExceptions.FoundTooMuch(modelDef.ModelFullName, item: SerializeUtil.ToJson(item));
                }
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }

            static void RestoreItem(T item, long oldTimestamp, string oldLastUser)
            {
                if (item is TimestampDbModel timestampDBModel)
                {
                    timestampDBModel.Deleted = false;
                    timestampDBModel.Timestamp = oldTimestamp;
                    timestampDBModel.LastUser = oldLastUser;
                }
            }
        }

        public async Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transactionContext = null) where T : TimelessDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(modelDef.ModelFullName, modelDef.DatabaseName);
            }

            try
            {
                WhereExpression<T> where = Where(whereExpr).And(t => !t.Deleted);

                var command = DbCommandBuilder.CreateDeleteCommand(EngineType, modelDef, where);

                await _databaseEngine.ExecuteCommandNonQueryAsync(transactionContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, whereExpr.ToString(), ex);
            }
        }

        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, item, modelDef);

            long oldTimestamp = -1;
            string oldLastUser = "";

            try
            {
                //Prepare
                if (item is TimestampDbModel timestampDBModel)
                {
                    oldTimestamp = timestampDBModel.Timestamp;
                    oldLastUser = timestampDBModel.LastUser;

                    timestampDBModel.Timestamp = TimeUtil.UtcNowTicks;
                    timestampDBModel.LastUser = lastUser;
                }

                EngineCommand command = DbCommandBuilder.CreateUpdateCommand(EngineType, modelDef, item, oldTimestamp);

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    //TODO: 这里返回0，一般是因为version不匹配，单也有可能是Id不存在，或者Deleted=1.
                    //可以改造SqlHelper中的update语句为如下，进行一般改造，排除上述可能。
                    //在原始的update语句，比如：update tb_userdirectorypermission set LastUser='TTTgdTTTEEST' where Id = uuid_to_bin('08da5b35-b123-2d4f-876c-6ee360db28c1') and Deleted = 0 and Version='0';
                    //后面select found_rows(), count(1) as 'exits' from tb_userdirectorypermission where Id = uuid_to_bin('08da5b35-b123-2d4f-876c-6ee360db28c1') and Deleted = 0;
                    //然后使用Reader读取，通过两个值进行判断。
                    //如果found_rows=1，正常返回
                    //如果found_rows=0, exists = 1, version冲突
                    //如果found_rows=0, exists = 0, 已删除

                    //不存在和Version冲突，统称为冲突，所以不用改，反正后续业务为了解决冲突也会重新select什么的，到时候可以判定是已经删掉了还是version冲突

                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(item), "");
                }

                throw DatabaseExceptions.FoundTooMuch(modelDef.ModelFullName, SerializeUtil.ToJson(item));
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }

            static void RestoreItem(T item, long oldTimestamp, string oldLastUser)
            {
                if (item is TimestampDbModel timestampDBModel)
                {
                    timestampDBModel.Timestamp = oldTimestamp;
                    timestampDBModel.LastUser = oldLastUser;
                }
            }
        }

        public async Task UpdateFieldsAsync<T>(
            object id,
            IList<(string propertyName, object? propertyValue)> propertyNameValues,
            long timestamp,
            string lastUser,
            TransactionContext? transContext) where T : TimestampDbModel, new()
        {
            if (propertyNameValues.Count <= 0)
            {
                return;
            }

            if (id is long longId && longId <= 0)
            {
                throw DatabaseExceptions.LongIdShouldBePositive(longId);
            }

            if (id is Guid guid && guid.IsEmpty())
            {
                throw DatabaseExceptions.GuidShouldNotEmpty();
            }

            if (timestamp <= 0)
            {
                throw DatabaseExceptions.TimestampShouldBePositive(timestamp);
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, id);

            try
            {
                EngineCommand command = DbCommandBuilder.CreateUpdateFieldsUsingTimestampCompareCommand(
                    engineType: EngineType,
                    modelDef: modelDef,
                    id: id,
                    oldTimestamp: timestamp,
                    newTimestamp: TimeUtil.UtcNowTicks,
                    lastUser: lastUser,
                    propertyNames: propertyNameValues.Select(t => t.propertyName).ToList(),
                    propertyValues: propertyNameValues.Select(t => t.propertyValue).ToList());

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, $"使用Timestamp版本的乐观锁，出现冲突。id:{id}, lastUser:{lastUser}, timestamp:{timestamp}, propertyValues:{SerializeUtil.ToJson(propertyNameValues)}", "");
                }
                else
                {
                    throw DatabaseExceptions.FoundTooMuch(modelDef.ModelFullName, $"id:{id}, timestamp:{timestamp}, propertyValues:{SerializeUtil.ToJson(propertyNameValues)}");
                }
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, $"id:{id}, timestamp:{timestamp}, propertyValues:{SerializeUtil.ToJson(propertyNameValues)}", ex);
            }
        }

        public async Task UpdateFieldsAsync<T>(object id, IList<(string propertyName, object? oldValue, object? newValue)> propertyNameOldNewValues, string lastUser, TransactionContext? transContext)
            where T : DbModel, new()
        {
            if (propertyNameOldNewValues.Count <= 0)
            {
                throw new ArgumentException("数量为空", nameof(propertyNameOldNewValues));
            }

            if (id is long longId && longId <= 0)
            {
                throw DatabaseExceptions.LongIdShouldBePositive(longId);
            }

            if (id is Guid guid && guid.IsEmpty())
            {
                throw DatabaseExceptions.GuidShouldNotEmpty();
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, id);

            try
            {
                EngineCommand command = DbCommandBuilder.CreateUpdateFieldsUsingOldNewCompareCommand(
                    EngineType,
                    modelDef,
                    id,
                    TimeUtil.UtcNowTicks,
                    lastUser,
                    propertyNameOldNewValues.Select(t => t.propertyName).ToList(),
                    propertyNameOldNewValues.Select(t => t.oldValue).ToList(),
                    propertyNameOldNewValues.Select(t => t.newValue).ToList());

                int matchedRows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);

                if (matchedRows == 1)
                {
                    return;
                }
                else if (matchedRows == 0)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, $"使用新旧值对比的乐观锁出现冲突。id:{id}, lastUser:{lastUser}, propertyOldNewValues:{SerializeUtil.ToJson(propertyNameOldNewValues)}", "");
                }
                else
                {
                    throw DatabaseExceptions.FoundTooMuch(modelDef.ModelFullName, $"id:{id}, lastUser:{lastUser}, propertyOldNewValues:{SerializeUtil.ToJson(propertyNameOldNewValues)}");
                }
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, $"id:{id}, lastUser:{lastUser}, propertyOldNewValues:{SerializeUtil.ToJson(propertyNameOldNewValues)}", ex);
            }
        }

        /// <summary>
        /// AddOrUpdate,即override,不检查Timestamp
        /// </summary>
        public async Task SetByIdAsync<T>(T item, /*string lastUser,*/ TransactionContext? transContext = null) where T : TimelessDbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(modelDef.ModelFullName, modelDef.DatabaseName);
            }

            //long oldTimestamp = -1;
            //string oldLastUser = "";

            try
            {
                ////Prepare
                //if (item is TimestampDbModel timestampDBModel)
                //{
                //    oldTimestamp = timestampDBModel.Timestamp;
                //    oldLastUser = timestampDBModel.LastUser;

                //    timestampDBModel.Timestamp = TimeUtil.UtcNowTicks;
                //    timestampDBModel.LastUser = lastUser;
                //}

                var command = DbCommandBuilder.CreateAddOrUpdateCommand(EngineType, modelDef, item, false);

                _ = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }
        }

        #endregion

        #region 批量更改(Write)

        /// <summary>
        /// BatchAddAsync，反应Version变化
        /// </summary>
        public async Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (_databaseEngine.DatabaseSettings.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchAdd超过批量操作的最大数目", items.Count(), lastUser);
            }

            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return new List<object>();
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, items, modelDef);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                //Prepare
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                IList<object> newIds = new List<object>();

                var command = DbCommandBuilder.CreateBatchAddCommand(EngineType, modelDef, items, transContext == null);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext?.Transaction,
                    modelDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                if (modelDef.IsIdAutoIncrement)
                {
                    while (reader.Read())
                    {
                        newIds.Add(reader.GetValue(0));
                    }

                    int num = 0;

                    foreach (var item in items)
                    {
                        ((ILongId)item).Id = Convert.ToInt64(newIds[num++], GlobalSettings.Culture);
                    }
                }
                else if (modelDef.IsIdGuid)
                {
                    foreach (var item in items)
                    {
                        newIds.Add(((IGuidId)item).Id);
                    }
                }
                else if (modelDef.IsIdLong)
                {
                    foreach (var item in items)
                    {
                        newIds.Add(((ILongId)item).Id);
                    }
                }

                return newIds;
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(items), ex);
            }

        }

        private static void PrepareBatchItems<T>(IEnumerable<T> items, string lastUser, List<long> oldTimestamps, List<string?> oldLastUsers, DbModelDef modelDef) where T : DbModel, new()
        {
            if (!modelDef.IsTimestampDBModel)
            {
                return;
            }

            long timestamp = TimeUtil.UtcNowTicks;

            foreach (var item in items)
            {
                if (item is TimestampDbModel tsItem)
                {
                    oldTimestamps.Add(tsItem.Timestamp);
                    oldLastUsers.Add(tsItem.LastUser);

                    tsItem.Timestamp = timestamp;
                    tsItem.LastUser = lastUser;
                }
            }
        }

        private static void RestoreBatchItems<T>(IEnumerable<T> items, IList<long> oldTimestamps, IList<string?> oldLastUsers, DbModelDef modelDef) where T : DbModel, new()
        {
            if (!modelDef.IsTimestampDBModel)
            {
                return;
            }

            for (int i = 0; i < items.Count(); ++i)
            {
                if (items.ElementAt(i) is TimestampDbModel tsItem)
                {
                    tsItem.Timestamp = oldTimestamps[i];
                    tsItem.LastUser = oldLastUsers[i] ?? "";
                }
            }
        }

        /// <summary>
        /// 批量更改，反应Version变化
        /// </summary>
        public async Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (_databaseEngine.DatabaseSettings.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchUpdate超过批量操作的最大数目", items.Count(), lastUser);
            }

            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, items, modelDef);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                var command = DbCommandBuilder.CreateBatchUpdateCommand(EngineType, modelDef, items, oldTimestamps, transContext == null);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext?.Transaction,
                    modelDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(items), "BatchUpdate");
                    }

                    count++;
                }

                if (count != items.Count())
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(items), "");
                }
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(items), ex);
            }
        }

        public async Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (_databaseEngine.DatabaseSettings.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchDelete超过批量操作的最大数目", items.Count(), lastUser);
            }

            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, items, modelDef);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                var command = DbCommandBuilder.CreateBatchDeleteCommand(EngineType, modelDef, items, oldTimestamps, transContext == null);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext?.Transaction,
                    modelDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int affected = reader.GetInt32(0);

                    if (affected != 1)
                    {
                        throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(items), $"not found the {count}th data item");
                    }

                    count++;
                }

                if (count != items.Count())
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(items), "");
                }
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ErrorCode == DatabaseErrorCodes.ExecuterError)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(items), ex);
            }
        }

        public async Task BatchAddOrUpdateByIdAsync<T>(IEnumerable<T> items, TransactionContext? transContext) where T : TimelessDbModel, new()
        {
            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.DatabaseNotWritable(modelDef.ModelFullName, SerializeUtil.ToJson(items));
            }

            try
            {
                var command = DbCommandBuilder.CreateBatchAddOrUpdateCommand(EngineType, modelDef, items, transContext == null);

                _ = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"Items:{SerializeUtil.ToJson(items)}";
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, detail, ex);
            }
        }

        #endregion

        private static void ThrowIfNotWriteable(DbModelDef modelDef)
        {
            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(type: modelDef.ModelFullName, database: modelDef.DatabaseName);
            }
        }

        private void TruncateLastUser<T>(ref string lastUser, T item, DbModelDef modelDef) where T : DbModel, new()
        {
            if (lastUser.Length > DefaultLengthConventions.MAX_LAST_USER_LENGTH)
            {
                object id = modelDef.IsIdLong ? ((ILongId)item).Id : modelDef.IsIdGuid ? ((IGuidId)item).Id : "None";
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

        private void TruncateLastUser<T>(ref string lastUser, IEnumerable<T> items, DbModelDef modelDef) where T : DbModel, new()
        {
            foreach (T item in items)
            {
                TruncateLastUser(ref lastUser, item, modelDef);
            }
        }
    }
}