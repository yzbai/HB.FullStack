
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
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
    public sealed partial class DefaultDatabase : IDatabase
    {
        private readonly ILogger _logger;

        private IDbSettingManager DbSettingManager { get; }

        public IDbModelDefFactory ModelDefFactory { get; }

        public IDbCommandBuilder DbCommandBuilder { get; }

        public ITransaction Transaction { get; }

        public DefaultDatabase(
            ILogger<DefaultDatabase> logger,
            IDbSettingManager dbSettingManager,
            IDbModelDefFactory modelDefFactory,
            IDbCommandBuilder commandBuilder,
            ITransaction transaction)
        {
            _logger = logger;

            ModelDefFactory = modelDefFactory;
            DbSettingManager = dbSettingManager;
            DbCommandBuilder = commandBuilder;
            Transaction = transaction;
        }

        #region Initialize

        /// <summary>
        /// 有几个DbSchema，就初始化几次
        /// 初始化，如果在服务端，请加全局分布式锁来初始化
        /// 返回是否真正执行了Migration
        /// </summary>
        public async Task<bool> InitializeAsync(DbSchema dbSchema, string? connectionString, IList<string>? slaveConnectionStrings, IEnumerable<Migration>? migrations)
        {
            using IDisposable? scope = _logger.BeginScope("数据库初始化");

            DbSettingManager.SetConnectionStringIfNeed(dbSchema, connectionString, slaveConnectionStrings);

            DbSetting dbSetting = DbSettingManager.GetDbSetting(dbSchema);

            IEnumerable<Migration>? curMigrations = migrations?.Where(m => m.DbSchema == dbSchema).ToList();

            await CreateTablesIfNeed(curMigrations, dbSetting).ConfigureAwait(false);

            bool haveExecutedMigration = await MigrateIfNeeded(curMigrations, dbSetting).ConfigureAwait(false);

            _logger.LogInformation("数据初{DbSchema}始化成功！, Version:{Version}", dbSchema, dbSetting.Version);

            return haveExecutedMigration;
        }

        private async Task CreateTablesIfNeed(IEnumerable<Migration>? migrations, DbSetting dbSetting)
        {
            if (!dbSetting.AutomaticCreateTable)
            {
                return;
            }

            var engine = DbSettingManager.GetDatabaseEngine(dbSetting.EngineType);
            var connectionString = DbSettingManager.GetConnectionString(dbSetting.DbSchema, true);

            var transContext = await Transaction.BeginTransactionAsync(dbSetting.DbSchema).ConfigureAwait(false);

            try
            {
                //TODO: 这里没有对slave库进行操作
                SystemInfo? sys = await GetSystemInfoAsync(dbSetting, transContext).ConfigureAwait(false);

                //表明是新数据库
                if (sys == null)
                {
                    Migration? initMigration = migrations?.Where(m => m.OldVersion == 0 && m.NewVersion == dbSetting.Version).FirstOrDefault();

                    if (initMigration == null && dbSetting.Version != 1)
                    {
                        await transContext.RollbackAsync().ConfigureAwait(false );
                        throw DatabaseExceptions.TableCreateError(dbSetting.Version, dbSetting.DbSchema,
                            $"要从头创建Tables，且Version不从1开始，那么必须提供 从 Version :{0} 到 Version：{dbSetting.Version} 的Migration.");
                    }

                    await CreateTablesByDbSchemaAsync(dbSetting, transContext).ConfigureAwait(false);

                    await SetSystemVersionAsync(dbSetting.Version, dbSetting, transContext).ConfigureAwait(false);

                    //初始化数据
                    if (initMigration != null)
                    {
                        await ApplyMigration(dbSetting, transContext, initMigration).ConfigureAwait(false);
                    }

                    _logger.LogInformation("自动创建了{DbSchema}的数据库表, Version:{Version}", dbSetting.DbSchema, dbSetting.Version);
                }

                await transContext.CommitAsync().ConfigureAwait(false);
            }
            catch (DatabaseException)
            {
                await transContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                await transContext.RollbackAsync().ConfigureAwait(false);

                throw DatabaseExceptions.TableCreateError(dbSetting.Version, dbSetting.DbSchema, "Unkown", ex);
            }
        }

        private async Task<bool> MigrateIfNeeded(IEnumerable<Migration>? migrations, DbSetting dbSetting)
        {
            if (migrations.IsNullOrEmpty())
            {
                return false;
            }

            if (migrations != null && migrations.Any(m => m.NewVersion <= m.OldVersion))
            {
                throw DatabaseExceptions.MigrateError("", "Migraion NewVersion <= OldVersion");
            }

            bool haveExecutedMigration = false;

            var engine = DbSettingManager.GetDatabaseEngine(dbSetting.EngineType);
            ConnectionString connectionString = DbSettingManager.GetConnectionString(dbSetting.DbSchema, true);

            //TODO: 这里没有对slave库进行操作
            var transContext = await Transaction.BeginTransactionAsync(dbSetting.DbSchema, System.Data.IsolationLevel.Serializable);

            try
            {
                SystemInfo? sys = await GetSystemInfoAsync(dbSetting, transContext).ConfigureAwait(false);

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
                        await ApplyMigration(dbSetting, transContext, migration).ConfigureAwait(false);
                        haveExecutedMigration = true;
                    }

                    await SetSystemVersionAsync(dbSetting.Version, dbSetting, transContext).ConfigureAwait(false);

                    _logger.LogInformation("{DbSchema} Migarate Finished. From {OldVersion} to {NewVersion}", dbSetting.DbSchema, sys.Version, dbSetting.Version);
                }

                await transContext.CommitAsync().ConfigureAwait(false);

                return haveExecutedMigration;
            }
            catch (DatabaseException)
            {
                await transContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                await transContext.RollbackAsync().ConfigureAwait(false);

                throw DatabaseExceptions.MigrateError(dbSetting.DbSchema, "未知Migration错误", ex);
            }
        }

        private async Task ApplyMigration(DbSetting dbSetting, TransactionContext transContext, Migration migration)
        {
            var engine = DbSettingManager.GetDatabaseEngine(dbSetting.EngineType);

            if (migration.SqlStatement.IsNotNullOrEmpty())
            {
                EngineCommand command = new EngineCommand(migration.SqlStatement);

                await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
            }

            if (migration.ModifyFunc != null)
            {
                await migration.ModifyFunc(this, transContext).ConfigureAwait(false);
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

        private async Task<SystemInfo?> GetSystemInfoAsync(DbSetting dbSetting, TransactionContext transContext)
        {
            bool isExisted = await IsTableExistsAsync(dbSetting, SystemInfoNames.SYSTEM_INFO_TABLE_NAME, transContext).ConfigureAwait(false);

            if (!isExisted)
            {
                return null;
            }

            var command = DbCommandBuilder.CreateSystemInfoRetrieveCommand(dbSetting.EngineType);

            var engine = DbSettingManager.GetDatabaseEngine(dbSetting.EngineType);

            using IDataReader reader = await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false);

            SystemInfo systemInfo = new SystemInfo(dbSetting.DbSchema);

            while (reader.Read())
            {
                systemInfo.Set(reader["Name"].ToString()!, reader["Value"].ToString()!);
            }

            return systemInfo;
        }

        private async Task SetSystemVersionAsync(int version, DbSetting dbSetting, TransactionContext transContext)
        {
            var command = DbCommandBuilder.CreateSystemVersionSetCommand(dbSetting.EngineType, dbSetting.DbSchema, version);

            var engine = DbSettingManager.GetDatabaseEngine(dbSetting.EngineType);

            await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
        }

        #endregion

        #region Table 管理

        public async Task<bool> IsTableExistsAsync(DbSetting dbSetting, string tableName, TransactionContext transContext)
        {
            var command = DbCommandBuilder.CreateIsTableExistCommand(dbSetting.EngineType, tableName);

            var engine = DbSettingManager.GetDatabaseEngine(dbSetting.EngineType);

            object? result = await engine.ExecuteCommandScalarAsync(transContext.Transaction, command).ConfigureAwait(false);

            return System.Convert.ToBoolean(result, Globals.Culture);
        }

        private async Task<int> CreateTableAsync(DbModelDef def, TransactionContext transContext, bool addDropStatement, int varcharDefaultLength)
        {
            var command = DbCommandBuilder.CreateTableCreateCommand(def, addDropStatement, varcharDefaultLength);

            var engine = DbSettingManager.GetDatabaseEngine(def.EngineType);

            _logger.LogInformation("Table创建：{CommandText}", command.CommandText);

            return await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
        }

        private async Task CreateTablesByDbSchemaAsync(DbSetting dbSetting, TransactionContext trans)
        {
            foreach (DbModelDef modelDef in ModelDefFactory.GetAllDefsByDbSchema(dbSetting.DbSchema))
            {
                await CreateTableAsync(
                    modelDef,
                    trans,
                    dbSetting.AddDropStatementWhenCreateTable,
                    DbSettingManager.GetVarcharDefaultLength(dbSetting.DbSchema)).ConfigureAwait(false);
            }
        }

        #endregion


        #region 条件构造

        public FromExpression<T> From<T>() where T : DbModel, new() => DbCommandBuilder.From<T>();

        public WhereExpression<T> Where<T>() where T : DbModel, new() => DbCommandBuilder.Where<T>();

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DbModel, new() => DbCommandBuilder.Where<T>(sqlFilter, filterParams);

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DbModel, new() => DbCommandBuilder.Where(predicate);

        #endregion


        #region Others

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
            if (!modelDef.DbWriteable)
            {
                throw DatabaseExceptions.NotWriteable(type: modelDef.ModelFullName, database: modelDef.DbSchema);
            }
        }

        private void TruncateLastUser(ref string lastUser)
        {
            if (lastUser == null)
            {
                throw new ArgumentNullException(lastUser);
            }

            if (lastUser.Length > DefaultLengthConventions.MAX_LAST_USER_LENGTH)
            {
                _logger.LogWarning("LastUser 截断. {LastUser}", lastUser);

                lastUser = lastUser[..DefaultLengthConventions.MAX_LAST_USER_LENGTH];
            }
        }

        #endregion

    }
}