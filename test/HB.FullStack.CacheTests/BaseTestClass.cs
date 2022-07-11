using HB.FullStack.BaseTest;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.Infrastructure.Redis.Cache;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using IDatabase = HB.FullStack.Database.IDatabase;

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]

namespace HB.FullStack.CacheTests
{
    [TestClass]
    public class BaseTestClass
    {
        public static string DbName { get; set; } = null!;

        public static IDatabase Db { get; set; } = null!;

        public static ICache Cache { get; set; } = null!;
        public static ConnectionMultiplexer RedisConnection { get; private set; } = null!;
        public static int DatabaseNumber { get; private set; }
        public static string ApplicationName { get; private set; } = null!;

        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext _)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
               .AddUserSecrets(typeof(BC).Assembly, optional:true);

            IConfiguration configuration = configurationBuilder.Build();

            //RedisCacheOptions roptions = new RedisCacheOptions();

            //configuration.GetSection("RedisCache").Bind(roptions);

            IServiceCollection services = new ServiceCollection();

            string dbName = $"s{TimeUtil.UtcNowUnixTimeSeconds}.db";

            services
                .AddIdGen(configuration.GetSection("IdGen"))
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                //.AddMemoryCache()
                .AddSQLite(options =>
                {
                    options.CommonSettings.Version = 1;

                    var connSettings = new DatabaseConnectionSettings
                    {
                        DatabaseName = dbName,
                        IsMaster = true
                    };

                    connSettings.ConnectionString = $"Data Source={connSettings.DatabaseName}";

                    options.Connections.Add(connSettings);
                })
                .AddRedisCache(configuration.GetSection("RedisCache"));
            //.AddRedisKVStore(configuration.GetSection("RedisKVStore"))
            //.AddRedisEventBus(configuration.GetSection("RedisEventBus"))
            //.AddMemoryLock()
            //.AddSingleRedisDistributedLock(configuration.GetSection("RedisLock"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            IDatabase database = serviceProvider.GetRequiredService<IDatabase>();

            await database.InitializeAsync();

            //Set Context
            DbName = dbName;
            Db = database;

            Cache = serviceProvider.GetRequiredService<ICache>();
            RedisConnection = ConnectionMultiplexer.Connect(configuration["RedisCache:ConnectionSettings:0:ConnectionString"]);
            DatabaseNumber = Convert.ToInt32(configuration["RedisCache:ConnectionSettings:0:DatabaseNumber"]);
            ApplicationName = configuration["RedisCache:ApplicationName"];
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            SqliteConnection.ClearAllPools();

            File.Delete(DbName);

            RedisConnection.Close();
        }
    }
}