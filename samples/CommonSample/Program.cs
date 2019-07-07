using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Hosting;

namespace HB.PresentFish.Tools
{

    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider Services { get; private set; }

        private static void ConfigureServices()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false);

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                configurationBuilder.AddUserSecrets<Program>();
            }

            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder=> {
                builder.AddConsole();
            });

            serviceCollection.AddMySQLEngine(Configuration.GetSection("MySQL"));
            serviceCollection.AddDatabase(Configuration.GetSection("Database"));

            serviceCollection.AddRedis(Configuration.GetSection("Redis"));
            serviceCollection.AddKVStore(Configuration.GetSection("KVStore"));
            serviceCollection.AddDistributedRedisCache(o =>
            {
                Configuration.GetSection("RedisCache").Bind(o);
            });

            serviceCollection.AddAliyunSms(Configuration.GetSection("AliyunSms"));

            serviceCollection.AddSingleton<IApplicationLifetime, Microsoft.Extensions.Hosting.Internal.ApplicationLifetime>();

            //serviceCollection.AddKafkaEngine(Configuration.GetSection("Kafka"));

            //serviceCollection.AddEventBus(Configuration.GetSection("EventBus"));

            //serviceCollection.AddSingleton<IEventHandler, TestEventHandler>();
            //serviceCollection.AddSingleton<IEventHandler, TestEventHandler2>();

            Services = serviceCollection.BuildServiceProvider();

         
            // Configure NLog
            //ILoggerFactory loggerFactory = Services.GetRequiredService<ILoggerFactory>();
            //loggerFactory.ConfigureNLog("nlog.config");
            //loggerFactory.AddNLog();
        }

       

        static void Main(string[] args)
        {
            ConfigureServices();

            //SmsTest.DoSmsTest();
            //SQLCreator.DoSQLCreator();

        }
       
    }

    
}
