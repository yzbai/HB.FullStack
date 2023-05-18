﻿global using System;

global using Microsoft.VisualStudio.TestTools.UnitTesting;

global using static HB.FullStack.BaseTest.ApiConstants;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Test;
using HB.FullStack.Database;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Implements;
using HB.FullStack.EventBus;
using HB.FullStack.EventBus.Abstractions;
using HB.FullStack.KVStore;
using HB.FullStack.Lock.Distributed;
using HB.FullStack.Lock.Memory;
using HB.Infrastructure.Redis.EventBus;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

//TODO: change to method level
[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.ClassLevel)]

namespace HB.FullStack.BaseTest
{
    [TestClass]
    public class BaseTestClass
    {
        public static IServiceProvider ServiceProvider { get; set; } = null!;

        public static IConfiguration Configuration { get; set; } = null!;

        #region Db

        public static IDatabase Db { get; set; } = null!;

        public static IDbConfigManager DbConfigManager { get; set; } = null!;

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

        #region Client

        public static IApiClient ApiClient { get; set; } = null!;

        public static ITokenPreferences PreferenceProvider { get; set; } = null!;

        #endregion

        [AssemblyInitialize]
        public static async Task BaseAssemblyInit(TestContext _)
        {
            ServiceProvider = BuildServices();

            #region Db

            Db = ServiceProvider.GetRequiredService<IDatabase>();
            DbConfigManager = ServiceProvider.GetRequiredService<IDbConfigManager>();
            Trans = ServiceProvider.GetRequiredService<ITransaction>();

            await DropSysInfoTableFirstForTest();

            var SqliteDbFileName = $"s{TimeUtil.UtcNowUnixTimeSeconds}{SecurityUtil.CreateRandomString(6)}.db";
            SqliteConnectionString = $"Data Source={SqliteDbFileName}";

            //这里会初始化所有数据库
            use global locker
            await Db.InitializeAsync(new DbInitContext[] { new DbInitContext { DbSchemaName = DbSchema_Sqlite, ConnectionString = SqliteConnectionString } }).ConfigureAwait(false);

            #endregion

            #region Cache

            Cache = ServiceProvider.GetRequiredService<ICache>();
            RedisConnection = StackExchange.Redis.ConnectionMultiplexer.Connect(Configuration["RedisCache:ConnectionSettings:0:ConnectionString"]!);
            RedisDbNumber = Convert.ToInt32(Configuration["RedisCache:ConnectionSettings:0:DatabaseNumber"]);
            ApplicationName = Configuration["RedisCache:ApplicationName"]!;

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

            #region Client

            ApiClient = ServiceProvider.GetRequiredService<IApiClient>();

            PreferenceProvider = ServiceProvider.GetRequiredService<ITokenPreferences>();

            #endregion
        }

        [AssemblyCleanup]
        public static void BaseAssemblyCleanup()
        {
            SqliteConnection.ClearAllPools();

            if (SqliteDbFileName.IsNotNullOrEmpty())
            {
                File.Delete(SqliteDbFileName);
            }

            RedisConnection.Close();
        }

        [ClassInitialize]
        public static void BaseClassInit(DbEngineType engineType)
        {
        }

        [ClassCleanup]
        public static void BaseClassCleanup()
        {
        }

        private static async Task DropSysInfoTableFirstForTest()
        {
            string sql = $"DROP TABLE if exists `{SystemInfoNames.SYSTEM_INFO_TABLE_NAME}`;";

            var mysqlSchema = DbConfigManager.GetDbSchema(DbSchema_Mysql);
            var mysqlEngine = mysqlSchema.Engine;

            await mysqlEngine.ExecuteCommandNonQueryAsync(mysqlSchema.GetMasterConnectionString(), new DbEngineCommand(sql));
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

            DbOptions dbOptions = new DbOptions();

            Configuration.GetSection("Database").Bind(dbOptions);

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
                .AddSingleRedisDistributedLock(Configuration.GetSection("RedisLock"))
                .AddSingleton<ITokenPreferences, PreferenceProviderStub>()
                .AddApiClient(options =>
                {
                    options.HttpClientTimeout = TimeSpan.FromSeconds(100);

                    options.TokenSiteSetting = new SiteSetting
                    {
                        BaseUrl = new Uri($"http://localhost:{Port}/api/")
                    };
                    options.OtherSiteSettings.Add(new SiteSetting
                    {
                        SiteName = ApiEndpointName,
                        Version = ApiVersion,
                        BaseUrl = new Uri($"http://localhost:{Port}/api/"),
                        Endpoints = new List<ResEndpoint> { }
                    });
                });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            Globals.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            return serviceProvider;
        }

        public static TestHttpServer StartHttpServer(params TestRequestHandler[] handlers)
        {
            TestHttpServer httpServer = new TestHttpServer(Port, new List<TestRequestHandler>(handlers));

            return httpServer;
        }
    }
}