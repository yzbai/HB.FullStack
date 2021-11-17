using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Data.Sqlite;

namespace HB.FullStack.Database.Tests
{
    [TestClass]
    public class ServiceInitialize
    {
        private static string? _dbName;

        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

            IConfiguration Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            _dbName = $"test{TimeUtil.UtcNow.Ticks}.db";

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
                        DatabaseName = _dbName,
                        ConnectionString = $"Data Source={_dbName}",
                        IsMaster = true
                    });
                });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = serviceProvider.GetRequiredService<ILogger<ServiceInitialize>>();
            serviceProvider.GetRequiredService<IDatabase>().InitializeAsync().Wait();
             
            testContext.Properties.Add("ServiceProvider", serviceProvider);
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            if (_dbName.IsNotNullOrEmpty())
            {
                SqliteConnection.ClearAllPools();
                File.Delete(_dbName);
            }
        }
    }
}