using System;
using System.Collections.Generic;
using System.Text;
using HB.FullStack.EventBus.Abstractions;
using HB.FullStack.KVStore;
using HB.Infrastructure.Redis.Direct;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace HB.Infrastructure.Redis.Test
{
    public class ServiceFixture
    {
        public IConfiguration Configuration { get; private set; }

        public IServiceProvider Services { get; private set; }

        public ServiceFixture()
        {
            NLog.LogManager.LoadConfiguration("nlog.config");

            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();

            serviceCollection.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog();
            });

            serviceCollection.AddRedisDatabase(Configuration.GetSection("RedisDatabase"));
            serviceCollection.AddRedisEventBus(Configuration.GetSection("RedisEventBus"));
            serviceCollection.AddRedisKVStore(Configuration.GetSection("RedisKVStore"));



            Services = serviceCollection.BuildServiceProvider();

            GlobalSettings.Logger = Services.GetRequiredService<ILogger<ServiceFixture>>();
        }

        public IRedisDatabase RedisDatabase
        {
            get
            {
                return Services.GetRequiredService<IRedisDatabase>();
            }
        }

        public IKVStore KVStore
        {
            get
            {
                return Services.GetRequiredService<IKVStore>();
            }
        }

        public IEventBus EventBus
        {
            get
            {
                return Services.GetRequiredService<IEventBus>();
            }
        }
    }
}
