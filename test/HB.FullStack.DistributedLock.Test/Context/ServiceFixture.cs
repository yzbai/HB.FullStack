using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Lock.Distributed;
using HB.FullStack.Lock.Memory;
using HB.Infrastructure.Redis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace HB.FullStack.DistributedLock.Test
{
    public class ServiceFixture
    {
        private const string _connectionString = "brlitetest.redis.rds.aliyuncs.com:6379,password=xMS22xtNPc&4RzgU,defaultDatabase=1";
        private readonly IServiceProvider _serviceProvider;
        public const string ApplicationName = "LockTest";
        public const string InstanceName = "Default";

        public ServiceFixture()
        {
            _serviceProvider = BuildServices();
        }

        private ServiceProvider BuildServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddOptions();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
                builder.AddDebug();
            });

            services.AddSingleRedisDistributedLock(options =>
            {
                options.ApplicationName = ApplicationName;
                options.ConnectionSetting = new RedisInstanceSetting
                {
                    InstanceName = InstanceName,
                    ConnectionString = _connectionString,
                    DatabaseNumber = 0
                };
            });

            services.AddMemoryCache();
            services.AddMemoryLock();

            ServiceProvider provider = services.BuildServiceProvider();

            GlobalSettings.Logger = provider.GetRequiredService<ILogger<ServiceFixture>>();

            RedisConnection = ConnectionMultiplexer.Connect(_connectionString);

            return provider;
        }

        public IDistributedLockManager DistributedLockManager => _serviceProvider.GetRequiredService<IDistributedLockManager>();

        public IMemoryLockManager MemoryLockManager => _serviceProvider.GetRequiredService<IMemoryLockManager>();

        public StackExchange.Redis.ConnectionMultiplexer RedisConnection { get; set; }

        public ILogger Logger => _serviceProvider.GetRequiredService<ILogger<ServiceFixture>>();
    }
}
