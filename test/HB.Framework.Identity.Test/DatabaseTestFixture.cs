using HB.Framework.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace HB.Component.Identity.Test
{
    public class DatabaseTestFixture
    {
        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider Services { get; private set; }

        public DatabaseTestFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("Test.json", optional: false);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();

            serviceCollection.AddLogging(builder => {
                builder.AddConsole();
            });

            serviceCollection.AddMySQLEngine(Configuration.GetSection("MySQL"));
            serviceCollection.AddDatabase(Configuration.GetSection("Database"));

            Services = serviceCollection.BuildServiceProvider();
        }

        public IDatabase Database => Services.GetRequiredService<IDatabase>();
    }
}
