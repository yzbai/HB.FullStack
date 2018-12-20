using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.KVStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HB.Framework.KVStore.Test
{
    public class ServiceFixture
    {
        public IConfiguration Configuration { get; private set; }

        public IServiceProvider Services { get; private set; }

        public ServiceFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();

            serviceCollection.AddLogging(builder => {
                builder.AddConsole();
            });

            serviceCollection.AddKVStore(Configuration.GetSection("KVStore"));
            serviceCollection.AddRedisEngine(Configuration.GetSection("RedisEngine"));

            Services = serviceCollection.BuildServiceProvider();
        }

        public IKVStore KVStore => this.Services.GetRequiredService<IKVStore>();
    }
}
