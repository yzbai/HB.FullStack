using HB.Framework.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace HB.Framework.DatabaseTests
{
    public class ServiceFixture
    {
        private readonly IServiceProvider _mySQLServices;

        private readonly IServiceProvider _sQLiteServices;

        public ServiceFixture()
        {
            _mySQLServices = BuildServices("MySQL");
            _sQLiteServices = BuildServices("SQLite");
        }

        private ServiceProvider BuildServices(string databaseType)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddOptions();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
                builder.AddDebug();
            });

            if (databaseType.Equals("MySQL", GlobalSettings.ComparisonIgnoreCase))
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

            if (databaseType.Equals("SQLite", GlobalSettings.ComparisonIgnoreCase))
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
            return services.BuildServiceProvider();
        }

        public IDatabase MySQL => _mySQLServices.GetRequiredService<IDatabase>();
        public ITransaction MySQLTransaction => _mySQLServices.GetRequiredService<ITransaction>();
        public IDatabase SQLite => _sQLiteServices.GetRequiredService<IDatabase>();
        public ITransaction SQLiteTransaction => _sQLiteServices.GetRequiredService<ITransaction>();

    }
}
