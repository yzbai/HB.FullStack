using HB.FullStack.Database;
using HB.Infrastructure.MySQL;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class BaseTestClass
    {
        public static IDatabase Db { get; set; } = null!;

        public static ITransaction Trans { get; set; } = null!;

        public static string ConnectionString { get; set; } = null!;

        [AssemblyInitialize]
        public static async Task AssemblyInit(TestContext _)
        {
            #region MySql

            IServiceProvider serviceProvider1 = BuildMySql();

            IDatabase database = serviceProvider1.GetRequiredService<IDatabase>();

            //删除之前的表结构
            string dbName = database.DatabaseNames.ElementAt(0);
            string sql = $"DROP TABLE if exists `{SystemInfoNames.SYSTEM_INFO_TABLE_NAME}`;";

            await database.DatabaseEngine.ExecuteCommandNonQueryAsync(null, dbName, new EngineCommand(sql));

            Db = database;
            Trans = serviceProvider1.GetRequiredService<ITransaction>();
            ConnectionString = serviceProvider1.GetRequiredService<IOptions<DatabaseOptions>>().Value.Connections[0].ConnectionString;

            await database.InitializeAsync().ConfigureAwait(false);

            #endregion
        }

        public static IServiceProvider BuildMySql()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
               .AddUserSecrets(typeof(HB.FullStack.BaseTest.BC).Assembly, optional: true);

            IConfiguration configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            services
                .AddIdGen(configuration.GetSection("IdGen"))
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddMySQL(configuration.GetSection("MySQL"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            Globals.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            return serviceProvider;
        }
    }
}