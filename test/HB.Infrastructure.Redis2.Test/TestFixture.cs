using HB.Framework.KVStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.Test
{
    public class TestFixture
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

            serviceCollection.AddSingleton<IApplicationLifetime, Microsoft.Extensions.Hosting.Internal.ApplicationLifetime>();

            serviceCollection.AddOptions();

            serviceCollection.AddLogging(builder => {
                builder.AddConsole();
            });

            serviceCollection.AddRedisEngine(Configuration.GetSection("Redis"));
            serviceCollection.AddKVStore(Configuration.GetSection("KVStore"));

            Services = serviceCollection.BuildServiceProvider();
        }

        public IKVStore KVStore => Services.GetRequiredService<IKVStore>();
    }
}
