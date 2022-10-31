using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Database
{
    //TODO: 增加数据库坏掉，自动切换, 比如屏蔽某个Slave，或者master切换到slave
    internal class DbManager : IDbManager
    {
        class DbManageUnit
        {
            public ushort SlaveAccessCount = 0;
            public readonly int SlaveCount;

            public DbSetting Setting;

            public DbManageUnit(DbSetting setting)
            {
                Setting = setting;
                SlaveCount = setting.SlaveConnectionStrings.Count;
            }
        }

        private readonly Dictionary<string, DbManageUnit> _dbNameDbSettings = new Dictionary<string, DbManageUnit>();
        private readonly Dictionary<string, DbManageUnit> _dbKindDbSettings = new Dictionary<string, DbManageUnit>();
        private readonly ILogger<DbManager> _logger;
        private readonly IEnumerable<IDatabaseEngine> _databaseEngines;

        private IDatabaseEngine? _mysqlEngine;
        private IDatabaseEngine? _sqliteEngine;

        public DbManager(ILogger<DbManager> logger, IOptions<DatabaseOptions> options, IEnumerable<IDatabaseEngine> databaseEngines)
        {
            DatabaseOptions _options = options.Value;
            _logger = logger;
            _databaseEngines = databaseEngines;

            RangeDbSettings();

            RangeEngines();

            void RangeDbSettings()
            {
                //Range DbSettings
                foreach (DbSetting dbSetting in _options.DbSettings)
                {
                    if (dbSetting.Version < 0)
                    {
                        throw DatabaseExceptions.DbSettingError(dbSetting.Version, dbSetting.DbName, "database Version must be 1");
                    }

                    if (dbSetting.DbName.IsNullOrEmpty() && dbSetting.DbName.IsNullOrEmpty())
                    {
                        throw DatabaseExceptions.DbSettingError(dbSetting.DbName, dbSetting.DbKind, "DbName和DbKind不能都为空");
                    }

                    if (dbSetting.DbName.IsNotNullOrEmpty())
                    {
                        _dbNameDbSettings[dbSetting.DbName] = new DbManageUnit(dbSetting);
                    }

                    if (dbSetting.DbKind.IsNotNullOrEmpty())
                    {
                        _dbKindDbSettings[dbSetting.DbKind] = new DbManageUnit(dbSetting);
                    }
                }
            }

            void RangeEngines()
            {
                //Range DatabaseEngines
                foreach (IDatabaseEngine engine in _databaseEngines)
                {
                    if (engine.EngineType == EngineType.SQLite)
                    {
                        _sqliteEngine = engine;
                    }
                    else if (engine.EngineType == EngineType.MySQL)
                    {
                        _mysqlEngine = engine;
                    }
                }
            }
        }

        private DbManageUnit GetDbManageUnit(DbModelDef modelDef) => modelDef.DbName != null ? _dbNameDbSettings[modelDef.DbName] : _dbKindDbSettings[modelDef.DbKind!];
        private DbManageUnit GetDbManageUnitByDbName(string dbName) => _dbNameDbSettings[dbName];
        private DbManageUnit GetDbManageUnitByDbKind(string dbKind) => _dbNameDbSettings[dbKind];

        private static ConnectionString GetSlaveConnectionString(DbManageUnit dbUnit)
        {
            //这里采取平均轮训的方法
            if (dbUnit.SlaveCount == 0)
            {
                return dbUnit.Setting.ConnectionString.ThrowIfNull($"{dbUnit.Setting.DbName ?? dbUnit.Setting.DbKind} 没有ConnectionString");
            }

            return dbUnit.Setting.SlaveConnectionStrings[dbUnit.SlaveAccessCount++ % dbUnit.SlaveCount];
        }
        private static ConnectionString GetConnectionString(bool useMaster, DbManageUnit unit)
        {
            return useMaster
                            ? unit.Setting.ConnectionString.ThrowIfNull($"{unit.Setting.DbName ?? unit.Setting.DbKind} 没有ConnectionString")
                            : GetSlaveConnectionString(unit);
        }

        public ConnectionString GetConnectionString(DbModelDef modelDef, bool useMaster)
        {
            DbManageUnit unit = GetDbManageUnit(modelDef);
            return GetConnectionString(useMaster, unit);
        }

        public ConnectionString GetConnectionStringByDbName(string dbName, bool useMaster)
        {
            DbManageUnit unit = GetDbManageUnitByDbName(dbName);

            return GetConnectionString(useMaster, unit);
        }

        public IDatabaseEngine GetDatabaseEngineByDbName(string dbName)
        {
            return GetDbManageUnitByDbName(dbName).Setting.EngineType switch
            {
                EngineType.MySQL => _mysqlEngine.ThrowIfNull("没有添加MySql"),
                EngineType.SQLite => _sqliteEngine.ThrowIfNull("没有添加Sqlite"),
                _ => throw new NotImplementedException(),
            };
        }

        public IDatabaseEngine GetDatabaseEngine(DbModelDef modelDef)
        {
            return modelDef.EngineType switch
            {
                EngineType.MySQL => _mysqlEngine.ThrowIfNull("没有添加MySql"),
                EngineType.SQLite => _sqliteEngine.ThrowIfNull("没有添加Sqlite"),
                _ => throw new NotImplementedException(),
            };
        }

        public int GetVarcharDefaultLength(DbModelDef modelDef)
        {
            int optionLength = GetDbManageUnit(modelDef).Setting.DefaultVarcharLength;

            return optionLength == 0 ? DefaultLengthConventions.DEFAULT_VARCHAR_LENGTH : optionLength;
        }

        public int GetMaxBatchNumber(DbModelDef modelDef)
        {
            return GetDbManageUnit(modelDef).Setting.MaxBatchNumber;
        }

        public bool GetDefaultTrulyDelete(DbModelDef modelDef)
        {
            return GetDbManageUnit(modelDef).Setting.DefaultTrulyDelete;
        }

        #region Initialize

        /// <summary>
        /// 初始化，如果在服务端，请加全局分布式锁来初始化
        /// 返回是否真正执行了Migration
        /// </summary>
        public async Task<bool> InitializeAsync(IEnumerable<Migration>? migrations, string? connectionString, IList<string>? slaveConnectionString)
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

        public void ReportFullConnectionStringAndDatabaseName()
        {
            //对于没有指定DbName和ConnectionString的DbSetting，后期总得补充好.
            //ModelDef 没必要补充
        }

        private async Task AutoCreateTablesIfBrandNewAsync(bool addDropStatement, IEnumerable<Migration>? initializeMigrations)
        {
            foreach (string databaseName in _databaseEngine.DatabaseNames)
            {
                TransactionContext transactionContext = await Transaction.BeginTransactionAsync(databaseName, IsolationLevel.Serializable).ConfigureAwait(false);

                try
                {
                    SystemInfo? sys = await GetSystemInfoAsync(databaseName, transactionContext.Transaction).ConfigureAwait(false);

                    //表明是新数据库
                    if (sys == null)
                    {
                        //要求新数据必须从version = 1开始
                        if (_databaseSettings.Version != 1)
                        {
                            await Transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                            throw DatabaseExceptions.TableCreateError(_databaseSettings.Version, databaseName, "Database does not exists, database Version must be 1");
                        }

                        await CreateTablesByDatabaseAsync(databaseName, transactionContext, addDropStatement).ConfigureAwait(false);

                        await UpdateSystemVersionAsync(databaseName, 1, transactionContext.Transaction).ConfigureAwait(false);

                        //初始化数据
                        IEnumerable<Migration>? curInitMigrations = initializeMigrations?.Where(m => m.DatabaseName.Equals(databaseName, Globals.ComparisonIgnoreCase)).ToList();

                        if (curInitMigrations.IsNotNullOrEmpty())
                        {
                            if (curInitMigrations.Count() > 1)
                            {
                                throw DatabaseExceptions.MigrateError(databaseName, "Database have more than one Initialize Migrations");
                            }

                            await ApplyMigration(databaseName, transactionContext, curInitMigrations.First()).ConfigureAwait(false);
                        }
                    }

                    await Transaction.CommitAsync(transactionContext).ConfigureAwait(false);
                }
                catch (DatabaseException)
                {
                    await Transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                    throw;
                }
                catch (Exception ex)
                {
                    await Transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

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
            foreach (DbModelDef modelDef in DefFactory.GetAllDefsByDatabase(databaseName))
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
                TransactionContext transactionContext = await Transaction.BeginTransactionAsync(databaseName, IsolationLevel.Serializable).ConfigureAwait(false);

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
                            .Where(m => m.DatabaseName.Equals(sys.DatabaseName, Globals.ComparisonIgnoreCase))
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

                    await Transaction.CommitAsync(transactionContext).ConfigureAwait(false);
                }
                catch (DatabaseException)
                {
                    await Transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                    throw;
                }
                catch (Exception ex)
                {
                    await Transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

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

            return System.Convert.ToBoolean(result, Globals.Culture);
        }

        internal async Task<SystemInfo?> GetSystemInfoAsync(string databaseName, IDbTransaction transaction)
        {
            bool isExisted = await IsTableExistsAsync(databaseName, SystemInfoNames.SYSTEM_INFO_TABLE_NAME, transaction).ConfigureAwait(false);

            if (!isExisted)
            {
                //return new SystemInfo(dbName) { Version = 0 };
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
    }
}
