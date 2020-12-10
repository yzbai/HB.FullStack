using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Distributed;
using HB.FullStack.Lock.Memory;
using HB.Infrastructure.Redis;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace HB.FullStack
{
    public class ServiceFixture
    {
        public IConfiguration Configuration { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public IServiceProvider ServiceProvider2 { get; private set; }

        public ServiceFixture()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false);

            Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            services
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddMemoryCache()
                .AddMySQL(Configuration.GetSection("MySQL"))
                .AddRedisCache(Configuration.GetSection("RedisCache"))
                .AddRedisKVStore(Configuration.GetSection("RedisKVStore"))
                .AddRedisEventBus(Configuration.GetSection("RedisEventBus"))
                .AddMemoryLock()
                .AddSingleRedisDistributedLock(Configuration.GetSection("RedisLock"));

            ServiceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = ServiceProvider.GetRequiredService<ILogger<ServiceFixture>>();

            IServiceCollection services2 = new ServiceCollection();

            services2
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddMemoryCache()
                .AddSQLite(options =>
                {
                    options.CommonSettings.Version = 1;

                    options.Connections.Add(new DatabaseConnectionSettings
                    {
                        DatabaseName = "sqlite_test2.db",
                        ConnectionString = "Data Source=sqlite_test2.db",
                        IsMaster = true
                    });
                })
                .AddRedisCache(Configuration.GetSection("RedisCache"))
                .AddRedisKVStore(Configuration.GetSection("RedisKVStore"))
                .AddRedisEventBus(Configuration.GetSection("RedisEventBus"))
                .AddMemoryLock()
                .AddSingleRedisDistributedLock(Configuration.GetSection("RedisLock")); ;

            ServiceProvider2 = services2.BuildServiceProvider();
        }
    }
}
