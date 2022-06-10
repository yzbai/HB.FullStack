using HB.FullStack.Lock.Distributed;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Reflection;

namespace HB.FullStack.LockTests
{
    [TestClass]
    public class BaseTestClass
    {
        public static IDistributedLockManager DistributedLockManager { get; set; } = null!;

        public static IMemoryLockManager MemoryLockManager { get; set; } = null!;

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
                .AddMemoryLock()
                .AddSingleRedisDistributedLock(configuration.GetSection("RedisLock"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = serviceProvider.GetRequiredService<ILogger<BaseTestClass>>();

            DistributedLockManager = serviceProvider.GetRequiredService<IDistributedLockManager>();
            MemoryLockManager = serviceProvider.GetRequiredService<IMemoryLockManager>();
        }
    }
}