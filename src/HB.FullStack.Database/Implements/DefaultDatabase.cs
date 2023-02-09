
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Implements;
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
    internal sealed partial class DefaultDatabase : IDatabase
    {
        private readonly ILogger _logger;

        private IDbSchemaManager _dbSchemaManager { get; }

        public IDbModelDefFactory ModelDefFactory { get; }

        public IDbCommandBuilder DbCommandBuilder { get; }

        public ITransaction Transaction { get; }

        public DefaultDatabase(
            ILogger<DefaultDatabase> logger,
            IDbSchemaManager dbSchemaManager,
            IDbModelDefFactory modelDefFactory,
            IDbCommandBuilder commandBuilder,
            ITransaction transaction)
        {
            _logger = logger;

            ModelDefFactory = modelDefFactory;
            _dbSchemaManager = dbSchemaManager;
            DbCommandBuilder = commandBuilder;
            Transaction = transaction;
        }

        #region Initialize

        /// <summary>
        /// 有几个DbSchema，就初始化几次
        /// 初始化，如果在服务端，请加全局分布式锁来初始化
        /// 返回是否真正执行了Migration
        /// </summary>
        public async Task<bool> InitializeAsync(string dbSchemaName, string? connectionString, IList<string>? slaveConnectionStrings, IEnumerable<Migration>? migrations)
        {
            using IDisposable? scope = _logger.BeginScope("数据库初始化");

            //1. 设置connectionString, 如果需要
            _dbSchemaManager.SetConnectionString(dbSchemaName, connectionString, slaveConnectionStrings);

            DbSchema dbSchema = _dbSchemaManager.GetDbSchema(dbSchemaName);
            IEnumerable<Migration>? curMigrations = migrations?.Where(m => m.DbSchemaName == dbSchemaName).ToList();

            //2. 创建新数据, 如果需要
            await CreateTablesIfNeed(curMigrations, dbSchema).ConfigureAwait(false);

            //3. 迁移, 如果需要
            bool haveExecutedMigration = await MigrateIfNeeded(curMigrations, dbSchema).ConfigureAwait(false);

            _logger.LogInformation("数据初{DbSchemaName}始化成功！, Version:{Version}", dbSchemaName, dbSchema.Version);

            return haveExecutedMigration;
        }

        private async Task CreateTablesIfNeed(IEnumerable<Migration>? migrations, DbSchema dbSchema)
        {
            if (!dbSchema.AutomaticCreateTable)
            {
                return;
            }

            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);
            var connectionString = _dbSchemaManager.GetConnectionString(dbSchema.Name, true);

            var transContext = await Transaction.BeginTransactionAsync(dbSchema.Name).ConfigureAwait(false);

            try
            {
                //TODO: 这里没有对slave库进行操作
                SystemInfo? sys = await GetSystemInfoAsync(dbSchema, transContext).ConfigureAwait(false);

                //表明是新数据库
                if (sys == null)
                {
                    Migration? initMigration = migrations?.Where(m => m.OldVersion == 0 && m.NewVersion == dbSchema.Version).FirstOrDefault();

                    if (initMigration == null && dbSchema.Version != 1)
                    {
                        await transContext.RollbackAsync().ConfigureAwait(false);
                        throw DbExceptions.TableCreateError(dbSchema.Version, dbSchema.Name,
                            $"要从头创建Tables，且Version不从1开始，那么必须提供 从 Version :{0} 到 Version：{dbSchema.Version} 的Migration.");
                    }

                    await CreateTablesByDbSchemaAsync(dbSchema, transContext).ConfigureAwait(false);

                    await SetSystemVersionAsync(dbSchema.Version, dbSchema, transContext).ConfigureAwait(false);

                    //初始化数据
                    if (initMigration != null)
                    {
                        await ApplyMigration(dbSchema, transContext, initMigration).ConfigureAwait(false);
                    }

                    _logger.LogInformation("自动创建了{DbSchemaName}的数据库表, Version:{Version}", dbSchema.Name, dbSchema.Version);
                }

                await transContext.CommitAsync().ConfigureAwait(false);
            }
            catch (DbException)
            {
                await transContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                await transContext.RollbackAsync().ConfigureAwait(false);

                throw DbExceptions.TableCreateError(dbSchema.Version, dbSchema.Name, "Unkown", ex);
            }
        }

        private async Task<bool> MigrateIfNeeded(IEnumerable<Migration>? migrations, DbSchema dbSchema)
        {
            if (migrations.IsNullOrEmpty())
            {
                return false;
            }

            if (migrations != null && migrations.Any(m => m.NewVersion <= m.OldVersion))
            {
                throw DbExceptions.MigrateError("", "Migraion NewVersion <= OldVersion");
            }

            bool haveExecutedMigration = false;

            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);
            ConnectionString connectionString = _dbSchemaManager.GetConnectionString(dbSchema.Name, true);

            //TODO: 这里没有对slave库进行操作
            var transContext = await Transaction.BeginTransactionAsync(dbSchema.Name, System.Data.IsolationLevel.Serializable);

            try
            {
                SystemInfo? sys = await GetSystemInfoAsync(dbSchema, transContext).ConfigureAwait(false);

                if (sys!.Version < dbSchema.Version)
                {
                    if (migrations == null)
                    {
                        throw DbExceptions.MigrateError(sys.DatabaseSchema, $"缺少 {sys.DatabaseSchema}的Migration.");
                    }

                    IEnumerable<Migration> curOrderedMigrations = migrations.OrderBy(m => m.OldVersion).ToList();

                    if (!IsMigrationSufficient(sys.Version, dbSchema.Version, curOrderedMigrations))
                    {
                        throw DbExceptions.MigrateError(sys.DatabaseSchema, $"Migrations not sufficient.{sys.DatabaseSchema}");
                    }

                    foreach (Migration migration in curOrderedMigrations)
                    {
                        await ApplyMigration(dbSchema, transContext, migration).ConfigureAwait(false);
                        haveExecutedMigration = true;
                    }

                    await SetSystemVersionAsync(dbSchema.Version, dbSchema, transContext).ConfigureAwait(false);

                    _logger.LogInformation("{DbSchemaName} Migarate Finished. From {OldVersion} to {NewVersion}", dbSchema.Name, sys.Version, dbSchema.Version);
                }

                await transContext.CommitAsync().ConfigureAwait(false);

                return haveExecutedMigration;
            }
            catch (DbException)
            {
                await transContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                await transContext.RollbackAsync().ConfigureAwait(false);

                throw DbExceptions.MigrateError(dbSchema.Name, "未知Migration错误", ex);
            }
        }

        private async Task ApplyMigration(DbSchema dbSchema, TransactionContext transContext, Migration migration)
        {
            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);

            if (migration.SqlStatement.IsNotNullOrEmpty())
            {
                DbEngineCommand command = new DbEngineCommand(migration.SqlStatement);

                await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
            }

            if (migration.ModifyFunc != null)
            {
                await migration.ModifyFunc(this, transContext).ConfigureAwait(false);
            }

            _logger.LogInformation("数据库Migration, {DbSchemaName}, from {OldVersion}, to {NewVersion}, {Sql}",
                dbSchema.Name, migration.OldVersion, migration.NewVersion, migration.SqlStatement);
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

        private async Task<SystemInfo?> GetSystemInfoAsync(DbSchema dbSchema, TransactionContext transContext)
        {
            bool isExisted = await IsTableExistsAsync(dbSchema, SystemInfoNames.SYSTEM_INFO_TABLE_NAME, transContext).ConfigureAwait(false);

            if (!isExisted)
            {
                return null;
            }

            var command = DbCommandBuilder.CreateSystemInfoRetrieveCommand(dbSchema.EngineType);

            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);

            using IDataReader reader = await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false);

            SystemInfo systemInfo = new SystemInfo(dbSchema.Name);

            while (reader.Read())
            {
                systemInfo.Set(reader["Name"].ToString()!, reader["Value"].ToString()!);
            }

            return systemInfo;
        }

        private async Task SetSystemVersionAsync(int version, DbSchema dbSchema, TransactionContext transContext)
        {
            var command = DbCommandBuilder.CreateSystemVersionSetCommand(dbSchema.EngineType, dbSchema.Name, version);

            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);

            await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
        }

        #endregion

        #region Table 管理

        public async Task<bool> IsTableExistsAsync(DbSchema dbSchema, string tableName, TransactionContext transContext)
        {
            var command = DbCommandBuilder.CreateIsTableExistCommand(dbSchema.EngineType, tableName);

            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);

            object? result = await engine.ExecuteCommandScalarAsync(transContext.Transaction, command).ConfigureAwait(false);

            return System.Convert.ToBoolean(result, Globals.Culture);
        }

        private async Task<int> CreateTableAsync(DbModelDef def, TransactionContext transContext, DbSchema dbSchema)
        {
            var engine = _dbSchemaManager.GetDatabaseEngine(def.EngineType);

            var command = DbCommandBuilder.CreateTableCreateCommand(
                def, 
                dbSchema.AddDropStatementWhenCreateTable, 
                dbSchema.DefaultVarcharFieldLength, 
                dbSchema.MaxVarcharFieldLength, 
                dbSchema.MaxMediumTextFieldLength);

            _logger.LogInformation("Table创建：{CommandText}", command.CommandText);

            return await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
        }

        private async Task CreateTablesByDbSchemaAsync(DbSchema dbSchema, TransactionContext trans)
        {
            foreach (DbModelDef modelDef in ModelDefFactory.GetAllDefsByDbSchema(dbSchema.Name))
            {
                await CreateTableAsync(modelDef, trans, dbSchema).ConfigureAwait(false);
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
                throw DbExceptions.NotWriteable(type: modelDef.ModelFullName, database: modelDef.DbSchemaName);
            }
        }

        

        #endregion

    }
}