using HB.FullStack.EventBus;
using HB.FullStack.EventBus.Abstractions;
using HB.Infrastructure.Redis.EventBus;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HB.Infrastructure.Redis.Test
{
    [TestClass]
    public class BaseTestClass
    {
        public static IEventBus EventBus { get; set; } = null!;
        public static IList<EventSchema> EventSchemas { get; private set; } = null!;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext _)
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
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddMemoryCache()
                .AddRedisEventBus(configuration.GetSection("RedisEventBus"))
                .AddMemoryLock()
                .AddSingleRedisDistributedLock(configuration.GetSection("RedisLock"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            Globals.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            EventBus = serviceProvider.GetRequiredService<IEventBus>();
            EventSchemas = serviceProvider.GetRequiredService<IOptions<RedisEventBusOptions>>().Value.EventBusSettings.EventSchemas;
        }
    }
}