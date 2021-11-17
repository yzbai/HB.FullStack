using HB.FullStack.Database;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class BaseTestClass
    {
        public static IDatabase Db { get; set; } = null!;

        public static ITransaction Trans { get; set; } = null!;

        public static string DbName { get; set; } = null!;

        [AssemblyInitialize]
        public static async Task AssemblyInit(TestContext _)
        {
            (IServiceProvider serviceProvider2, string sqliteDbName) = BuildSqlite();

            IDatabase sqliteDb = serviceProvider2.GetRequiredService<IDatabase>();

            Db = sqliteDb;
            Trans = serviceProvider2.GetRequiredService<ITransaction>();
            DbName = sqliteDbName;

            await sqliteDb.InitializeAsync().ConfigureAwait(false);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            SqliteConnection.ClearAllPools();

            File.Delete(DbName);
        }

        public static (IServiceProvider serviceProvider, string dbName) BuildSqlite()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

            IConfiguration configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            string dbName = $"s{TimeUtil.UtcNowUnixTimeSeconds}{SecurityUtil.CreateRandomString(6)}.db";

            services
                .AddIdGen(configuration.GetSection("IdGen"))
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
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
                });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            return (serviceProvider, dbName);
        }
    }
}