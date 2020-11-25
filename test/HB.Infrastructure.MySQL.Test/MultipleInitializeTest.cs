using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Database;
using HB.Infrastructure.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HB.Framework.DatabaseTests
{
    public class MultipleInitializeTest
    {
        private const string _connectionString = "127.0.0.1:6379";
        public const string ApplicationName = "Test";
        public const string InstanceName = "Default";

        [Fact]
        public async Task Test_ConcurrenceAsync()
        {
            IServiceProvider sp = BuildServices();

            IDatabase database = sp.GetRequiredService<IDatabase>();

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 20; ++i)
            {
                tasks.Add(database.InitializeAsync());
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private static IServiceProvider BuildServices()
        {
            ServiceCollection services = new ServiceCollection();

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
                    InstanceName = InstanceName,
                    ConnectionString = _connectionString,
                    DatabaseNumber = 0
                };
            });


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


            IServiceProvider serviceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = serviceProvider.GetRequiredService<ILogger<MutipleTableTest>>();

            return serviceProvider;
        }
    }
}
