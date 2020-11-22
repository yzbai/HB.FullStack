using HB.Infrastructure.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace HB.Framework.Cache.Test
{
    public class ServiceFixture
    {
        private readonly IServiceProvider _serviceProvider;

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
                options.ApplicationName = "Test";
                options.ConnectionSettings.Add(new RedisInstanceSetting
                {
                    InstanceName = "Default",
                    ConnectionString = "127.0.0.1:6379",
                    DatabaseNumber = 0
                });
            });

            ServiceProvider provider = services.BuildServiceProvider();

            GlobalSettings.Logger = provider.GetRequiredService<ILogger<ServiceFixture>>();

            return provider;
        }

        public ICache Cache => _serviceProvider.GetRequiredService<ICache>();


    }
}
