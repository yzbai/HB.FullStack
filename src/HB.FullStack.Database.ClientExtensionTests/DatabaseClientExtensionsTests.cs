using Microsoft.VisualStudio.TestTools.UnitTesting;
using HB.FullStack.Database;

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using HB.FullStack.Database.SQL;
using System.Linq;

namespace HB.FullStack.Database.Tests
{
    [TestClass()]
    public class DatabaseClientExtensionsTests
    {
        public TestContext TestContext { get; set; } = null!;

        public IDatabase Db => ((IServiceProvider)TestContext.Properties["ServiceProvider"]!).GetRequiredService<IDatabase>();
        public ITransaction Trans =>((IServiceProvider)TestContext.Properties["ServiceProvider"]!).GetRequiredService<ITransaction>();

        [TestMethod()]
        public async Task AddOrUpdateByIdAsyncTestAsync()
        {
            var lst = Mocker.GetCExtEntities(1);
            CExtEntity entity = lst[0];

            await Db.AddOrUpdateByIdAsync(entity, "Test", null);

            Assert.AreEqual(entity.Version, 0);

            await Db.AddOrUpdateByIdAsync(entity, "Test", null);

            Assert.AreEqual(entity.Version, 1);
        }

        [TestMethod()]
        public async Task DeleteAsyncTestAsync()
        {
            var lst = Mocker.GetCExtEntities();

            var trans = await Trans.BeginTransactionAsync<CExtEntity>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(lst, "Tests", trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }


            await Db.DeleteAsync<CExtEntity>(e => SqlStatement.In(e.Id, false, lst.Select(e => (object)e.Id).ToArray())).ConfigureAwait(false);

            long count = await Db.CountAsync<CExtEntity>(e => SqlStatement.In(e.Id, true, lst.Select(e => (object)e.Id).ToArray()), null).ConfigureAwait(false);

            Assert.AreEqual(count, 0);
        }
    }

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