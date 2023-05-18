global using System;

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
using Microsoft.VisualStudio.Threading;

//TODO: change to method level
[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.ClassLevel)]

namespace HB.FullStack.BaseTest
{
    [TestClass]
    public class BaseTestClass
    {
        public IServiceProvider ServiceProvider { get; set; } = null!;

        public IConfiguration Configuration { get; set; } = null!;

        #region Db

        public IDatabase Db { get; set; } = null!;

        public const string DbSchema_Mysql = "mysql_test";
        public const string DbSchema_Sqlite = "sqlite_test";

        public IDbConfigManager DbConfigManager { get; set; } = null!;

        public ITransaction Trans { get; set; } = null!;

        public string SqliteConnectionString = null!;

        public string SqliteDbFileName = null!;

        #endregion

        #region KVStore

        public IKVStore KVStore { get; set; } = null!;

        #endregion

        #region Cache

        public ICache Cache { get; set; } = null!;
        public StackExchange.Redis.ConnectionMultiplexer RedisConnection { get; private set; } = null!;
        public int RedisDbNumber { get; private set; }
        public string ApplicationName { get; private set; } = null!;

        #endregion

        #region Lock

        public IDistributedLockManager DistributedLockManager { get; set; } = null!;

        public IMemoryLockManager MemoryLockManager { get; set; } = null!;

        #endregion

        #region EventBus

        public IEventBus EventBus { get; set; } = null!;
        public IList<EventSchema> EventSchemas { get; private set; } = null!;

        #endregion

        #region Client

        public IApiClient ApiClient { get; set; } = null!;

        public ITokenPreferences PreferenceProvider { get; set; } = null!;

        #endregion

        public BaseTestClass(DbEngineType defaultEngineType)
        {
            ServiceProvider = BuildServices(defaultEngineType);

            Globals.Logger = ServiceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            #region Db

            Db = ServiceProvider.GetRequiredService<IDatabase>();
            DbConfigManager = ServiceProvider.GetRequiredService<IDbConfigManager>();
            Trans = ServiceProvider.GetRequiredService<ITransaction>();

            InitializeDatabaseAsync().WaitWithoutInlining();

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

        private async Task InitializeDatabaseAsync()
        {
            //await DropSysInfoTableFirstForTest();

            var SqliteDbFileName = $"s{TimeUtil.UtcNowUnixTimeSeconds}{SecurityUtil.CreateRandomString(6)}.db";
            SqliteConnectionString = $"Data Source={SqliteDbFileName}";

            Globals.Logger.LogInformation($"测试开始初始化数据库");

            IDistributedLock distributedLock = await DistributedLockManager.LockAsync(
                resource: nameof(DbSchema_Mysql),
                expiryTime: TimeSpan.FromSeconds(5 * 60),
                waitTime: TimeSpan.FromSeconds(6 * 60)).ConfigureAwait(false);

            try
            {
                if (!distributedLock.IsAcquired)
                {
                    Globals.Logger.LogInformation("无法获取初始化数据库的锁，可能其他站点正在进行初始化");
                    throw new Exception("等待之后，依然无法获得初始化数据库的锁.");
                }

                Globals.Logger.LogInformation("获取了初始化数据库的锁");

                await Db.InitializeAsync(new DbInitContext[] {
                    new DbInitContext
                    {
                        DbSchemaName = DbSchema_Sqlite,
                        ConnectionString = SqliteConnectionString
                    } }).ConfigureAwait(false);
            }
            finally
            {
                await distributedLock.DisposeAsync().ConfigureAwait(false);
            }
        }

        ~BaseTestClass()
        {
            SqliteConnection.ClearAllPools();

            if (SqliteDbFileName.IsNotNullOrEmpty())
            {
                File.Delete(SqliteDbFileName);
            }

            RedisConnection.Close();
        }

        //private async Task DropSysInfoTableFirstForTest()
        //{
        //    string sql = $"DROP TABLE if exists `{SystemInfoNames.SYSTEM_INFO_TABLE_NAME}`;";

        //    var mysqlSchema = DbConfigManager.GetDbSchema(DbSchema_Mysql);
        //    var mysqlEngine = mysqlSchema.Engine;

        //    await mysqlEngine.ExecuteCommandNonQueryAsync(mysqlSchema.GetMasterConnectionString(), new DbEngineCommand(sql));
        //}

        public IServiceProvider BuildServices(DbEngineType defaultEngineType)
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
                .AddDatabase(dbOptions =>
                {
                    Configuration.GetSection("Database").Bind(dbOptions);

                    bool flag = true;
                    foreach (var schema in dbOptions.DbSchemas)
                    {
                        if (flag && schema.EngineType == defaultEngineType)
                        {
                            schema.IsDefault = true;
                            flag = false;
                        }
                        else
                        {
                            schema.IsDefault = false;
                        }
                    }
                }, builder => { builder.AddMySQL().AddSQLite(); })
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