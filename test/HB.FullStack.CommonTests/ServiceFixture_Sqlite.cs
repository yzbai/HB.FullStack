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
        /// <exception cref="DatabaseException">Ignore.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
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

                    var connSettings = new DatabaseConnectionSettings
                    {
                        DatabaseName = $"s{TimeUtil.UtcNowUnixTimeSeconds}.db",
                        IsMaster = true
                    };

                    connSettings.ConnectionString = $"Data Source={connSettings.DatabaseName}";

                    options.Connections.Add(connSettings);

                });

            ServiceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = ServiceProvider.GetRequiredService<ILogger<ServiceFixture_Sqlite>>();
            ServiceProvider.GetRequiredService<IDatabase>().InitializeAsync().Wait();

            GlobalSettings.Logger.LogDebug($"当前Process,{Environment.ProcessId}");
        }
    }
}
