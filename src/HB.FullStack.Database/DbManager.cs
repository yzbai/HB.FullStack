global using DbSchema = System.String;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace HB.FullStack.Database
{
    //TODO: 增加数据库坏掉，自动切换, 比如屏蔽某个Slave，或者master切换到slave
    //TODO: 记录Settings到tb_sys_info表中，自动加载
    internal class DbManager : IDbManager
    {
        class DbManageUnit
        {
            public ushort SlaveAccessCount = 0;
            public int SlaveCount;

            public DbSetting Setting;

            public DbManageUnit(DbSetting setting)
            {
                Setting = setting;
                SlaveCount = setting.SlaveConnectionStrings == null ? 0 : setting.SlaveConnectionStrings.Count;
            }
        }

        private readonly ILogger<DbManager> _logger;
        private readonly IEnumerable<IDatabaseEngine> _databaseEngines;
        private readonly ITransaction _transaction;
        private readonly IDbCommandBuilder _commandBuilder;
        private readonly IDbModelDefFactory _defFactory;
        private readonly IDatabaseEngine? _mysqlEngine;
        private readonly IDatabaseEngine? _sqliteEngine;
        private readonly Dictionary<DbSchema, DbManageUnit> _dbManageUnits = new Dictionary<string, DbManageUnit>();

        public DbManager(ILogger<DbManager> logger, IOptions<DatabaseOptions> options, IEnumerable<IDatabaseEngine> databaseEngines, ITransaction transaction, IDbCommandBuilder commandBuilder, IDbModelDefFactory defFactory)
        {
            DatabaseOptions _options = options.Value;
            _logger = logger;
            _databaseEngines = databaseEngines;
            _transaction = transaction;
            _commandBuilder = commandBuilder;
            _defFactory = defFactory;

            //Range DbSettings
            foreach (DbSetting dbSetting in _options.DbSettings)
            {
                if (dbSetting.Version < 0)
                {
                    throw DatabaseExceptions.DbSettingError(dbSetting.Version, dbSetting.DbSchema, "database Version must be 1");
                }

                if (dbSetting.DbSchema.IsNullOrEmpty())
                {
                    throw new ArgumentNullException("DbSetting中DbSchema不应该为空");
                }

                _dbManageUnits[dbSetting.DbSchema] = new DbManageUnit(dbSetting);
            }

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

        #region Settings

        public ConnectionString GetConnectionString(DbSchema dbSchema, bool useMaster)
        {
            DbManageUnit unit = _dbManageUnits[dbSchema];

            return useMaster
                ? unit.Setting.ConnectionString.ThrowIfNull($"{unit.Setting.DbSchema} 没有ConnectionString")
                : GetSlaveConnectionString(unit);

            static ConnectionString GetSlaveConnectionString(DbManageUnit dbUnit)
            {
                //这里采取平均轮训的方法
                if (dbUnit.SlaveCount == 0)
                {
                    return dbUnit.Setting.ConnectionString.ThrowIfNull($"{dbUnit.Setting.DbSchema} 没有ConnectionString");
                }

                return dbUnit.Setting.SlaveConnectionStrings![dbUnit.SlaveAccessCount++ % dbUnit.SlaveCount];
            }
        }

        public IDatabaseEngine GetDatabaseEngine(DbSchema dbSchema) => GetDatabaseEngine(_dbManageUnits[dbSchema].Setting.EngineType);

        public IDatabaseEngine GetDatabaseEngine(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => _mysqlEngine.ThrowIfNull("没有添加MySql"),
                EngineType.SQLite => _sqliteEngine.ThrowIfNull("没有添加Sqlite"),
                _ => throw new NotImplementedException(),
            };
        }

        public int GetVarcharDefaultLength(DbSchema dbSchema)
        {
            int optionLength = _dbManageUnits[dbSchema].Setting.DefaultVarcharLength;

            return optionLength == 0 ? DefaultLengthConventions.DEFAULT_VARCHAR_LENGTH : optionLength;
        }

        public int GetMaxBatchNumber(DbSchema dbSchema)
        {
            return _dbManageUnits[dbSchema].Setting.MaxBatchNumber;
        }

        public bool GetDefaultTrulyDelete(DbSchema dbSchema)
        {
            return _dbManageUnits[dbSchema].Setting.DefaultTrulyDelete;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// 有几个DbSchema，就初始化几次
        /// 初始化，如果在服务端，请加全局分布式锁来初始化
        /// 返回是否真正执行了Migration
        /// </summary>
        public async Task<bool> InitializeAsync(DbSchema dbSchema, string? connectionString, IList<string>? slaveConnectionStrings, IEnumerable<Migration>? migrations)
        {
            using IDisposable? scope = _logger.BeginScope("数据库初始化");

            DbManageUnit unit = _dbManageUnits[dbSchema];

            FillConnectionStringIfNeed(connectionString, slaveConnectionStrings, unit);

            IEnumerable<Migration>? curMigrations = migrations?.Where(m => m.DbSchema == dbSchema).ToList();

            await CreateTablesIfNeed(curMigrations, unit).ConfigureAwait(false);

            bool haveExecutedMigration = await MigrateIfNeeded(curMigrations, unit).ConfigureAwait(false);

            _logger.LogInformation("数据初{DbSchema}始化成功！, Version:{Version}", dbSchema, unit.Setting.Version);

            return haveExecutedMigration;
        }

        private static void FillConnectionStringIfNeed(string? connectionString, IList<string>? slaveConnectionStrings, DbManageUnit unit)
        {
            //补充ConnectionString，不替换
            if (unit.Setting.ConnectionString == null)
            {
                unit.Setting.ConnectionString = new ConnectionString(connectionString.ThrowIfNullOrEmpty($"在初始化时，应该为 {unit.Setting.DbSchema} 提供连接字符串"));
            }

            //补充SlaveConnectionString,不替换
            if (unit.Setting.SlaveConnectionStrings == null && slaveConnectionStrings != null)
            {
                unit.Setting.SlaveConnectionStrings = slaveConnectionStrings.Select(c => new ConnectionString(c)).ToList();
                unit.SlaveCount = slaveConnectionStrings.Count;
            }
        }

        private async Task CreateTablesIfNeed(IEnumerable<Migration>? migrations, DbManageUnit unit)
        {
            if (!unit.Setting.AutomaticCreateTable)
            {
                return;
            }

            TransactionContext transactionContext = await _transaction.BeginTransactionAsync(unit.Setting.DbSchema, System.Data.IsolationLevel.Serializable).ConfigureAwait(false);
            DbSetting dbSetting = unit.Setting;

            try
            {
                SystemInfo? sys = await GetSystemInfoAsync(dbSetting, transactionContext.Transaction).ConfigureAwait(false);

                //表明是新数据库
                if (sys == null)
                {
                    Migration? initMigration = migrations?.Where(m => m.OldVersion == 0 && m.NewVersion == dbSetting.Version).FirstOrDefault();

                    if (initMigration == null && dbSetting.Version != 1)
                    {
                        await transactionContext.RollbackAsync().ConfigureAwait(false);
                        throw DatabaseExceptions.TableCreateError(dbSetting.Version, dbSetting.DbSchema,
                            $"要从头创建Tables，且Version不从1开始，那么必须提供 从 Version :{0} 到 Version：{dbSetting.Version} 的Migration.");
                    }

                    await CreateTablesByDbSchemaAsync(dbSetting, transactionContext).ConfigureAwait(false);

                    await SetSystemVersionAsync(dbSetting.Version, dbSetting, transactionContext.Transaction).ConfigureAwait(false);

                    //初始化数据
                    if (initMigration != null)
                    {
                        await ApplyMigration(dbSetting, transactionContext, initMigration).ConfigureAwait(false);
                    }

                    _logger.LogInformation("自动创建了{DbSchema}的数据库表, Version:{Version}", dbSetting.DbSchema, dbSetting.Version);
                }

                await transactionContext.CommitAsync().ConfigureAwait(false);
            }
            catch (DatabaseException)
            {
                await transactionContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                await transactionContext.RollbackAsync().ConfigureAwait(false);

                throw DatabaseExceptions.TableCreateError(dbSetting.Version, dbSetting.DbSchema, "Unkown", ex);
            }
        }

        private async Task<bool> MigrateIfNeeded(IEnumerable<Migration>? migrations, DbManageUnit unit)
        {
            if (migrations.IsNullOrEmpty())
            {
                return false;
            }

            if (migrations != null && migrations.Any(m => m.NewVersion <= m.OldVersion))
            {
                throw DatabaseExceptions.MigrateError("", "Migraion NewVersion <= OldVersion");
            }

            DbSetting dbSetting = unit.Setting;
            bool haveExecutedMigration = false;

            TransactionContext transactionContext = await _transaction.BeginTransactionAsync(dbSetting.DbSchema, System.Data.IsolationLevel.Serializable).ConfigureAwait(false);

            try
            {
                SystemInfo? sys = await GetSystemInfoAsync(dbSetting, transactionContext.Transaction).ConfigureAwait(false);

                if (sys!.Version < dbSetting.Version)
                {
                    if (migrations == null)
                    {
                        throw DatabaseExceptions.MigrateError(sys.DatabaseSchema, $"缺少 {sys.DatabaseSchema}的Migration.");
                    }

                    IEnumerable<Migration> curOrderedMigrations = migrations.OrderBy(m => m.OldVersion).ToList();

                    if (!IsMigrationSufficient(sys.Version, dbSetting.Version, curOrderedMigrations))
                    {
                        throw DatabaseExceptions.MigrateError(sys.DatabaseSchema, $"Migrations not sufficient.{sys.DatabaseSchema}");
                    }

                    foreach (Migration migration in curOrderedMigrations)
                    {
                        await ApplyMigration(dbSetting, transactionContext, migration).ConfigureAwait(false);
                        haveExecutedMigration = true;
                    }

                    await SetSystemVersionAsync(dbSetting.Version, dbSetting, transactionContext.Transaction).ConfigureAwait(false);

                    _logger.LogInformation("{DbSchema} Migarate Finished. From {OldVersion} to {NewVersion}", dbSetting.DbSchema, sys.Version, dbSetting.Version);
                }

                await transactionContext.CommitAsync().ConfigureAwait(false);

                return haveExecutedMigration;
            }
            catch (DatabaseException)
            {
                await transactionContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                await transactionContext.RollbackAsync().ConfigureAwait(false);

                throw DatabaseExceptions.MigrateError(dbSetting.DbSchema, "未知Migration错误", ex);
            }
        }

        private async Task ApplyMigration(DbSetting dbSetting, TransactionContext transactionContext, Migration migration)
        {
            var engine = GetDatabaseEngine(dbSetting.EngineType);

            if (migration.SqlStatement.IsNotNullOrEmpty())
            {
                EngineCommand command = new EngineCommand(migration.SqlStatement);

                await engine.ExecuteCommandNonQueryAsync(transactionContext.Transaction, command).ConfigureAwait(false);
            }

            if (migration.ModifyFunc != null)
            {
                await migration.ModifyFunc(engine, transactionContext).ConfigureAwait(false);
            }

            _logger.LogInformation("数据库Migration, {DbShema}, from {OldVersion}, to {NewVersion}, {Sql}",
                dbSetting.DbSchema, migration.OldVersion, migration.NewVersion, migration.SqlStatement);
        }

        /// <summary>
        /// 检查是否依次提供了不中断的Migration
        /// </summary>
        private static bool IsMigrationSufficient(int startVersion, int endVersion, IEnumerable<Migration> curOrderedMigrations)
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

        #region SystemInfo 管理

        public async Task<SystemInfo?> GetSystemInfoAsync(DbSetting dbSetting, IDbTransaction transaction)
        {
            bool isExisted = await IsTableExistsAsync(dbSetting, SystemInfoNames.SYSTEM_INFO_TABLE_NAME, transaction).ConfigureAwait(false);

            if (!isExisted)
            {
                //return new SystemInfo(dbName) { Version = 0 };
                return null;
            }

            var command = _commandBuilder.CreateSystemInfoRetrieveCommand(dbSetting.EngineType);

            var engine = GetDatabaseEngine(dbSetting.EngineType);

            using IDataReader reader = await engine.ExecuteCommandReaderAsync(transaction, command).ConfigureAwait(false);

            SystemInfo systemInfo = new SystemInfo(dbSetting.DbSchema);

            while (reader.Read())
            {
                systemInfo.Set(reader["Name"].ToString()!, reader["Value"].ToString()!);
            }

            return systemInfo;
        }

        public async Task SetSystemVersionAsync(int version, DbSetting dbSetting, IDbTransaction transaction)
        {
            var command = _commandBuilder.CreateSystemVersionSetCommand(dbSetting.EngineType, dbSetting.DbSchema, version);

            var engine = GetDatabaseEngine(dbSetting.EngineType);

            await engine.ExecuteCommandNonQueryAsync(transaction, command).ConfigureAwait(false);
        }

        #endregion

        #region Table 管理

        public async Task<bool> IsTableExistsAsync(DbSetting dbSetting, string tableName, IDbTransaction transaction)
        {
            var command = _commandBuilder.CreateIsTableExistCommand(dbSetting.EngineType, tableName);

            var engine = GetDatabaseEngine(dbSetting.EngineType);

            object? result = await engine.ExecuteCommandScalarAsync(transaction, command).ConfigureAwait(false);

            return System.Convert.ToBoolean(result, Globals.Culture);
        }

        private async Task<int> CreateTableAsync(DbModelDef def, TransactionContext transContext, bool addDropStatement, int varcharDefaultLength)
        {
            var command = _commandBuilder.CreateTableCreateCommand(def, addDropStatement, varcharDefaultLength);

            var engine = GetDatabaseEngine(def.EngineType);

            _logger.LogInformation("Table创建：{CommandText}", command.CommandText);

            return await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
        }

        private async Task CreateTablesByDbSchemaAsync(DbSetting dbSetting, TransactionContext transactionContext)
        {
            foreach (DbModelDef modelDef in _defFactory.GetAllDefsByDbSchema(dbSetting.DbSchema))
            {
                await CreateTableAsync(
                    modelDef,
                    transactionContext,
                    dbSetting.AddDropStatementWhenCreateTable,
                    GetVarcharDefaultLength(dbSetting.DbSchema)).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
