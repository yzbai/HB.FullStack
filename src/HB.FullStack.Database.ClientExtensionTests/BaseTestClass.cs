using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using System.Reflection;

namespace HB.FullStack.Database.Tests
{
    [TestClass]
    public class BaseTestClass
    {
        public static IDatabase Db { get; set; } = null!;

        public static ITransaction Trans { get; set; } = null!;

        public static string DbName { get; set; } = null!;

        [AssemblyInitialize]
        public static async Task Initialize(TestContext _)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
               .AddUserSecrets(typeof(HB.FullStack.BaseTest.BC).Assembly, optional: true);

            IConfiguration Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            DbName = $"test{TimeUtil.UtcNow.Ticks}.db";

            services
                .AddIdGen(Configuration.GetSection("IdGen"))
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddSQLite(options =>
                {
                    options.CommonSettings.Version = 1;

                    options.Connections.Add(new DatabaseConnectionSettings
                    {
                        DatabaseName = DbName,
                        ConnectionString = $"Data Source={DbName}",
                        IsMaster = true
                    });
                });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            Db = serviceProvider.GetRequiredService<IDatabase>();
            Trans = serviceProvider.GetRequiredService<ITransaction>();

            await Db.InitializeAsync();
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            SqliteConnection.ClearAllPools();
            File.Delete(DbName);
        }
    }
}