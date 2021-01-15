using System;
using System.Diagnostics;
using HB.FullStack.Database;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HB.FullStack
{
    public class ServiceFixture_Sqlite
    {
        public IConfiguration Configuration { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <exception cref="DatabaseException"></exception>
        public ServiceFixture_Sqlite()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .AddUserSecrets<ServiceFixture_Sqlite>()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

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
                .AddSQLite(options =>
                {
                    options.CommonSettings.Version = 1;

                    options.Connections.Add(new DatabaseConnectionSettings
                    {
                        DatabaseName = "sqlite_test2.db",
                        ConnectionString = "Data Source=sqlite_test2.db",
                        IsMaster = true
                    });
                });

            ServiceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = ServiceProvider.GetRequiredService<ILogger<ServiceFixture_Sqlite>>();
            ServiceProvider.GetRequiredService<IDatabase>().InitializeAsync().Wait();

            //GlobalSettings.Logger.LogInformation($"当前Process,{Environment.ProcessId}");
        }
    }
}
