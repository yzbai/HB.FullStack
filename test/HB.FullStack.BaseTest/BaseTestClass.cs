﻿global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using static HB.FullStack.BaseTest.BaseTestClass;
global using HB.FullStack.BaseTest;

using HB.FullStack.Database;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Lock.Distributed;
using HB.FullStack.Lock.Memory;
using HB.FullStack.KVStore;
using HB.FullStack.EventBus.Abstractions;
using HB.FullStack.EventBus;
using HB.Infrastructure.Redis.EventBus;
using Microsoft.Extensions.Options;

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]



namespace HB.FullStack.BaseTest
{
    [TestClass]
    public class BaseTestClass
    {
        public static IServiceProvider ServiceProvider { get; set; } = null!;

        public static IConfiguration Configuration { get; set; } = null!;

        #region Db

        public const string DbSchema_Mysql = "mysql_test";
        public const string DbSchema_Sqlite = "sqlite_test";

        public static IDatabase Db { get; set; } = null!;

        public static IDbManager DbManager { get; set; } = null!;

        public static ITransaction Trans { get; set; } = null!;

        public static string SqliteConnectionString = null!;

        public static string SqliteDbFileName = null!;

        #endregion

        #region KVStore

        public static IKVStore KVStore { get; set; } = null!;

        #endregion

        #region Cache

        public static ICache Cache { get; set; } = null!;
        public static StackExchange.Redis.ConnectionMultiplexer RedisConnection { get; private set; } = null!;
        public static int RedisDbNumber { get; private set; }
        public static string ApplicationName { get; private set; } = null!;

        #endregion

        #region Lock

        public static IDistributedLockManager DistributedLockManager { get; set; } = null!;

        public static IMemoryLockManager MemoryLockManager { get; set; } = null!;

        #endregion

        #region EventBus


        public static IEventBus EventBus { get; set; } = null!;
        public static IList<EventSchema> EventSchemas { get; private set; } = null!;

        #endregion


        [AssemblyInitialize]
        public static async Task AssemblyInit(TestContext _)
        {
            ServiceProvider = BuildServices();

            #region Db

            Db = ServiceProvider.GetRequiredService<IDatabase>();
            DbManager = ServiceProvider.GetRequiredService<IDbManager>();
            Trans = ServiceProvider.GetRequiredService<ITransaction>();

            //初始化 DbSchema_Mysql
            string sql = $"DROP TABLE if exists `{SystemInfoNames.SYSTEM_INFO_TABLE_NAME}`;";

            var mysqlEngine = DbManager.GetDatabaseEngine(DbSchema_Mysql);

            await mysqlEngine.ExecuteCommandNonQueryAsync(DbManager.GetConnectionString(DbSchema_Mysql, true), new EngineCommand(sql));

            await Db.InitializeAsync(DbSchema_Mysql, null, null, null).ConfigureAwait(false);

            //初始化 DbSchema_Sqlite
            var SqliteDbFileName = $"s{TimeUtil.UtcNowUnixTimeSeconds}{SecurityUtil.CreateRandomString(6)}.db";
            SqliteConnectionString = $"Data Source={SqliteDbFileName}";

            await Db.InitializeAsync(DbSchema_Sqlite, SqliteConnectionString, null, null).ConfigureAwait(false);

            #endregion

            #region Cache

            Cache = ServiceProvider.GetRequiredService<ICache>();
            RedisConnection = StackExchange.Redis.ConnectionMultiplexer.Connect(Configuration["RedisCache:ConnectionSettings:0:ConnectionString"]);
            RedisDbNumber = Convert.ToInt32(Configuration["RedisCache:ConnectionSettings:0:DatabaseNumber"]);
            ApplicationName = Configuration["RedisCache:ApplicationName"];

            #endregion

            #region Lock

            DistributedLockManager = ServiceProvider.GetRequiredService<IDistributedLockManager>();
            MemoryLockManager = ServiceProvider.GetRequiredService<IMemoryLockManager>();

            #endregion

            #region KVStore

            KVStore = ServiceProvider.GetRequiredService<IKVStore>();

            #endregion

            #region EventBus
            
            EventBus = ServiceProvider.GetRequiredService<IEventBus>();
            EventSchemas = ServiceProvider.GetRequiredService<IOptions<RedisEventBusOptions>>().Value.EventBusSettings.EventSchemas;

            #endregion
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            SqliteConnection.ClearAllPools();

            File.Delete(SqliteDbFileName);

            RedisConnection.Close();
        }

        public static IServiceProvider BuildServices()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
               .AddUserSecrets(typeof(BaseTestClass).Assembly, optional: true);

            Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            services
                .AddIdGen(Configuration.GetSection("IdGen"))
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddDatabase(Configuration.GetSection("Database"), builder => { builder.AddMySQL().AddSQLite(); })
                .AddRedisCache(Configuration.GetSection("RedisCache"))
                .AddRedisKVStore(Configuration.GetSection("RedisKVStore"))
                .AddRedisEventBus(Configuration.GetSection("RedisEventBus"))
                .AddMemoryLock()
                .AddSingleRedisDistributedLock(Configuration.GetSection("RedisLock"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            Globals.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            return serviceProvider;
        }
    }
}