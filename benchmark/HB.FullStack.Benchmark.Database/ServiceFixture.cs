using System;

using HB.FullStack.Database;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HB.FullStack
{
    public class ServiceFixture
    {
        public IConfiguration Configuration { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public IServiceProvider ServiceProvider2 { get; private set; }

        public ServiceFixture()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false);

            Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            services
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddMySQL(Configuration.GetSection("MySQL"));

            ServiceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = ServiceProvider.GetRequiredService<ILogger<ServiceFixture>>();

            IServiceCollection services2 = new ServiceCollection();

            services2
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

            ServiceProvider2 = services2.BuildServiceProvider();
        }
    }
}
