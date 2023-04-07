using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.Config;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using IDatabase = HB.FullStack.Database.IDatabase;

namespace HB.FullStack.Server.Startup
{
    /// <summary>
    /// 初始化任务包括：
    /// 1， Db初始化
    /// 2， Cache清理
    /// </summary>
    public class InitHostedService : IHostedService
    {
        private readonly IDatabase _database;
        private readonly IDistributedLockManager _lockManager;
        private readonly ICache _cache;
        private readonly InitHostedServiceOptions _options;

        public InitHostedService(IOptions<InitHostedServiceOptions> options, IDatabase database, IDistributedLockManager lockManager, ICache cache)
        {
            _database        = database;
            _lockManager     = lockManager;
            _cache           = cache;
            _options         = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeDatabaseAsync().ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        private async Task InitializeDatabaseAsync()
        {
            Globals.Logger.LogInformation("InitHostedService 开始初始化数据库");

            IDistributedLock distributedLock = await _lockManager.LockAsync(
                resource: nameof(InitHostedService),
                expiryTime: TimeSpan.FromSeconds(_options.DbInitLockExpireSeconds),
                waitTime: TimeSpan.FromSeconds(_options.DbInitLockWaitSeconds)).ConfigureAwait(false);

            try
            {
                if (!distributedLock.IsAcquired)
                {
                    Globals.Logger.LogInformation("无法获取初始化数据库的锁，可能其他站点正在进行初始化");
                }

                Globals.Logger.LogInformation("获取了初始化数据库的锁");

                await _database.InitializeAsync(_options.DbInitContexts).ConfigureAwait(false);
            }
            finally
            {
                await distributedLock.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}