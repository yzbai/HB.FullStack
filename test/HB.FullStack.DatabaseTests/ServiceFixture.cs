using HB.FullStack.Database;
using HB.FullStack.Database.Engine;
using HB.Infrastructure.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.DatabaseTests
{
    public static class ServiceFixture
    {
        private const string _connectionString = "127.0.0.1:6379";
        public const string ApplicationName = "Test";
        public const string InstanceName = "Default";

        private static readonly IServiceProvider _mySQLserviceProvider;
        private static readonly IServiceProvider _sqliteserviceProvider;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        static ServiceFixture()
        {
            _mySQLserviceProvider = BuildServices(DatabaseEngineType.MySQL);
            _sqliteserviceProvider = BuildServices(DatabaseEngineType.SQLite);


            //GlobalSettings.Logger = _mySQLserviceProvider.GetRequiredService<ILogger<ServiceFixture>>();
            GlobalSettings.Logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            GlobalSettings.Logger.LogInformation("ServiceFixture初始化");

            List<Task> tasks = new List<Task>();
            tasks.Add(MySQL.InitializeAsync());
            tasks.Add(SQLite.InitializeAsync());

            Task.WhenAll(tasks).Wait();
        }

        private static IServiceProvider BuildServices(DatabaseEngineType engineType)
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
                    InstanceName = InstanceName + engineType.ToString(),
                    ConnectionString = _connectionString,
                    DatabaseNumber = 0
                };
            });

            if (engineType == DatabaseEngineType.MySQL)
            {
                services.AddMySQL(options =>
                {
                    options.CommonSettings.Version = 1;

                    options.Connections.Add(new DatabaseConnectionSettings
                    {
                        DatabaseName = "test_db",
                        ConnectionString = "server=rm-bp16d156f2r6b78438o.mysql.rds.aliyuncs.com;port=3306;user=brlite_test;password=EgvfXB2eWucbtm0C;database=test_db;SslMode=None;",
                        IsMaster = true
                    });
                });
            }

            if (engineType == DatabaseEngineType.SQLite)
            {
                services.AddSQLite(options =>
                {
                    options.CommonSettings.Version = 1;

                    options.Connections.Add(new DatabaseConnectionSettings
                    {
                        DatabaseName = "test2.db",
                        ConnectionString = "Data Source=test2.db",
                        IsMaster = true
                    });
                });
            }

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        public static IDatabase MySQL => _mySQLserviceProvider.GetRequiredService<IDatabase>();
        public static ITransaction MySQLTransaction => _mySQLserviceProvider.GetRequiredService<ITransaction>();

        public static IDatabase SQLite => _sqliteserviceProvider.GetRequiredService<IDatabase>();
        public static ITransaction SQLiteTransaction => _sqliteserviceProvider.GetRequiredService<ITransaction>();

    }
}
