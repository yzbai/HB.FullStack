using HB.Framework.KVStore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.Test
{
    public class TestFixture : IDisposable
    {
        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider Services { get; private set; }

        public TestFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("Test.json", optional: false);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IApplicationLifetime, ApplicationLifetime>();

            serviceCollection.AddOptions();

            serviceCollection.AddLogging(builder => {
                builder.AddConsole();
            });

            serviceCollection.AddRedisEngine(Configuration.GetSection("Redis"));
            serviceCollection.AddKVStore(Configuration.GetSection("KVStore"));

            Services = serviceCollection.BuildServiceProvider();
        }

        public IKVStore GetKVStore()
        {
            return Services.GetRequiredService<IKVStore>();
        }

        public void Dispose()
        {

        }
    }
}
