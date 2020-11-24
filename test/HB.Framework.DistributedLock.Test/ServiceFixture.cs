using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Cache;
using HB.Infrastructure.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HB.Framework.DistributedLock.Test
{
    public class ServiceFixture
    {
        private const string _connectionString = "127.0.0.1:6379";
        private readonly IServiceProvider _serviceProvider;
        public const string ApplicationName = "Test";
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

            ServiceProvider provider = services.BuildServiceProvider();

            GlobalSettings.Logger = provider.GetRequiredService<ILogger<ServiceFixture>>();

            RedisConnection = ConnectionMultiplexer.Connect(_connectionString);

            return provider;
        }

        public IDistributedLockManager DistributedLockManager => _serviceProvider.GetRequiredService<IDistributedLockManager>();

        public StackExchange.Redis.ConnectionMultiplexer RedisConnection { get; set; }

        public ILogger Logger => _serviceProvider.GetRequiredService<ILogger<ServiceFixture>>();
    }
}
