using HB.FullStack.KVStore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;

namespace HB.FullStack.KVStoreTests
{
    [TestClass]
    public class BaseTestClass
    {
        public static IKVStore KVStore { get; set; } = null!;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext _)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

            IConfiguration configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            services
                .AddOptions()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddRedisKVStore(configuration.GetSection("RedisKVStore"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            KVStore = serviceProvider.GetRequiredService<IKVStore>();
        }
    }
}