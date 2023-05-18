using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

using HB.FullStack.Database.Config;
using HB.FullStack.Database.Implements;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {

        //TODO: 思考要不要加一个waithandler，只能不在初始化中才能进行操作?

        /// <summary>
        /// 有几个DbSchema，就初始化几次
        /// 初始化，如果在服务端，请加全局分布式锁来初始化
        /// 返回是否真正执行了Migration
        /// </summary>
        public async Task InitializeAsync(IList<DbInitContext>? initContexts = null)
        {
            using IDisposable? scope = _logger.BeginScope("数据库初始化");

            Dictionary<string, DbInitContext> rangedContexts = RangeInitContexts(initContexts, _dbConfigManager.InitContexts);

            foreach (DbSchema dbSchema in _dbConfigManager.AllDbSchemas)
            {
                if (rangedContexts.TryGetValue(dbSchema.Name, out var dbInitContext))
                {
                    dbSchema.SetConnectionString(dbInitContext?.ConnectionString, dbInitContext?.SlaveConnectionStrings);
                }

                if (dbSchema.ConnectionString == null)
                {
                    continue;
                }

                IEnumerable<Migration>? curMigrations = dbInitContext?.Migrations?.Where(m => m.DbSchemaName == dbSchema.Name).ToList();

                //2. 创建新数据, 如果需要
                await CreateTablesIfNeed(curMigrations, dbSchema).ConfigureAwait(false);

                //3. 迁移, 如果需要
                await MigrateIfNeeded(curMigrations, dbSchema).ConfigureAwait(false);

                _logger.LogInformation("数据初{DbSchemaName}始化成功！, Version:{Version}", dbSchema.Name, dbSchema.Version);
            }

            static Dictionary<string, DbInitContext> RangeInitContexts(IList<DbInitContext>? contexts, IEnumerable<DbInitContext>? contextsFromOptions)
            {
                Dictionary<string, DbInitContext> dict = new Dictionary<string, DbInitContext>();

                if (contextsFromOptions.IsNotNullOrEmpty())
                {
                    //覆盖contextsFromOptions
                    foreach (var context in contextsFromOptions)
                    {
                        dict[context.DbSchemaName] = context;
                    }
                }

                if(contexts.IsNotNullOrEmpty())
                {
                    foreach(var context in contexts)
                    {
                        dict[context.DbSchemaName] = context;
                    }
                }

                return dict;
            }
        }


        public void SetConnectionString(string schemaName, string? connectionString, IList<string>? slaveConnectionStrings)
        {
            var schema = _dbConfigManager.GetDbSchema(schemaName);
            schema.SetConnectionString(connectionString, slaveConnectionStrings);
        }

        private async Task CreateTablesIfNeed(IEnumerable<Migration>? migrations, DbSchema dbSchema)
        {
            if (!dbSchema.AutomaticCreateTable)
            {
                return;
            }

            var transContext = await Transaction.BeginTransactionAsync(dbSchema).ConfigureAwait(false);

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

            //TODO: 这里没有对slave库进行操作
            var transContext = await Transaction.BeginTransactionAsync(dbSchema, System.Data.IsolationLevel.Serializable);

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

            /// <summary>
            /// 检查是否依次提供了不中断的Migration
            /// </summary>
            static bool IsMigrationSufficient(int startVersion, int endVersion, IEnumerable<Migration> curOrderedMigrations)
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
        }

        private async Task ApplyMigration(DbSchema dbSchema, TransactionContext transContext, Migration migration)
        {
            if (migration.SqlStatement.IsNotNullOrEmpty())
            {
                DbEngineCommand command = new DbEngineCommand(migration.SqlStatement);

                await dbSchema.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
            }

            if (migration.ModifyFunc != null)
            {
                await migration.ModifyFunc(this, transContext).ConfigureAwait(false);
            }

            //TODO: 这里掺和了Cache的操作
            if (migration.CacheCleanTask != null)
            {
                await migration.CacheCleanTask().ConfigureAwait(false);
            }

            _logger.LogInformation("数据库Migration, {DbSchemaName}, from {OldVersion}, to {NewVersion}, {Sql}",
                dbSchema.Name, migration.OldVersion, migration.NewVersion, migration.SqlStatement);
        }
    }
}
