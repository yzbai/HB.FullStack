using HB.Framework.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace HB.Component.Identity.Test
{
    public class ServiceFixture
    {
        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider Services { get; private set; }

        public ServiceFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();

            serviceCollection.AddLogging(builder => {
                builder.AddConsole();
            });

            serviceCollection.AddSQLite(Configuration.GetSection("SQLite"));

            Services = serviceCollection.BuildServiceProvider();
        }

        public IDatabase Database => Services.GetRequiredService<IDatabase>();


    }
}
