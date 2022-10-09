
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Extensions;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
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
        private readonly DbCommonSettings _databaseSettings;
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

            return System.Convert.ToBoolean(result, GlobalSettings.Culture);
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

        private static void PrepareBatchItems<T>(IEnumerable<T> items, string lastUser, List<long> oldTimestamps, List<string?> oldLastUsers, DbModelDef modelDef) where T : DbModel, new()
        {
            if (!modelDef.IsTimestampDBModel)
            {
                return;
            }

            long timestamp = TimeUtil.Timestamp;

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

        private static void ThrowIfNotWriteable(DbModelDef modelDef)
        {
            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(type: modelDef.ModelFullName, database: modelDef.DatabaseName);
            }
        }

        private void TruncateLastUser(ref string lastUser)
        {
            if (lastUser.Length > DefaultLengthConventions.MAX_LAST_USER_LENGTH)
            {
                _logger.LogWarning("LastUser 截断. {LastUser}", lastUser);

                lastUser = lastUser[..DefaultLengthConventions.MAX_LAST_USER_LENGTH];
            }
        }
    }
}