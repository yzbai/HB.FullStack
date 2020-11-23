using HB.Infrastructure.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace HB.Framework.Cache.Test
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

            services.AddRedisCache(options =>
            {
                options.ApplicationName = ApplicationName;
                options.ConnectionSettings.Add(new RedisInstanceSetting
                {
                    InstanceName = InstanceName,
                    ConnectionString = _connectionString,
                    DatabaseNumber = 0
                });
            });

            ServiceProvider provider = services.BuildServiceProvider();

            GlobalSettings.Logger = provider.GetRequiredService<ILogger<ServiceFixture>>();

            RedisConnection = ConnectionMultiplexer.Connect(_connectionString);

            return provider;
        }

        public ICache Cache => _serviceProvider.GetRequiredService<ICache>();

        public StackExchange.Redis.ConnectionMultiplexer RedisConnection { get; set; }
    }
}
