using HB.PresentFish.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;

namespace SQLGenerator
{
    class Program
    {

        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider Services { get; private set; }

        private static void ConfigureServices()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();


            serviceCollection.AddLogging();

            serviceCollection.AddMySQLEngine(Configuration.GetSection("MySQL"));
            serviceCollection.AddDatabase(Configuration.GetSection("Database"));


            Services = serviceCollection.BuildServiceProvider();

            // Configure NLog
            ILoggerFactory loggerFactory = Services.GetRequiredService<ILoggerFactory>();
            loggerFactory.ConfigureNLog("nlog.config");
            loggerFactory.AddNLog();
        }

        static void Main(string[] args)
        {
            ConfigureServices();

            SQLCreator();
        }

        static void SQLCreator()
        {
            //Tool1: Create the sql statement for database
            SQLCreator startUp = ActivatorUtilities.CreateInstance<SQLCreator>(Services);
        }
    }
}
