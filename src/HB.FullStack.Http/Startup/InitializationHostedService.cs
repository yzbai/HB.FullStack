using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IDatabase = HB.FullStack.Database.IDatabase;

namespace HB.FullStack.WebApi.Startup
{
    /// <summary>
    /// 初始化任务包括：
    /// 1， Db初始化
    /// 2， Cache清理
    /// </summary>
    public class InitializationHostedService : IHostedService
    {
        private readonly IDatabase _database;
        private readonly IDistributedLockManager _lockManager;
        private readonly ICache _cache;
        private readonly InitializationOptions _context;

        public InitializationHostedService(IDatabase database, IDistributedLockManager lockManager, ICache cache, InitializationOptions context)
        {
            _database = database;
            _lockManager = lockManager;
            _cache = cache;
            _context = context;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var dbContext in _context.DbInitializeContexts)
            {
                bool haveMigrationExecuted = await InitializeDatabaseAsync(dbContext.DbSchemaName, dbContext.Migrations).ConfigureAwait(false);

                if (haveMigrationExecuted)
                {
                    //TODO: clear the cache
                    //清理比如xxx开头的CacheItem,要求Cache有统一开头，且不能与KVStore冲突。所以KVStore最好与cache是不同的实例

                    dbContext.CacheCleanAction?.Invoke(_cache);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 返回是否有Migration被执行
        /// </summary>
        public async Task<bool> InitializeDatabaseAsync(string dbSchemaName, IEnumerable<Migration>? migrations)
        {
            Globals.Logger.LogDebug("开始初始化数据库:{DbSchemaName}", dbSchemaName);

            IDistributedLock distributedLock = await _lockManager.LockAsync(
                resource: dbSchemaName,
                expiryTime: TimeSpan.FromMinutes(5),
                waitTime: TimeSpan.FromMinutes(10)).ConfigureAwait(false);

            try
            {
                if (!distributedLock.IsAcquired)
                {
                    throw WebApiExceptions.DatabaseInitLockError(dbSchemaName);
                }

                Globals.Logger.LogDebug("获取了初始化数据库的锁:{DbSchemaName}", dbSchemaName);

                return await _database.InitializeAsync(dbSchemaName, null, null, migrations).ConfigureAwait(false);
            }
            finally
            {
                await distributedLock.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}