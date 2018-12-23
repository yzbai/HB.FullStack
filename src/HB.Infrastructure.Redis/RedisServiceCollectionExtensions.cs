using HB.Framework.EventBus;
using HB.Framework.KVStore.Engine;
using HB.Infrastructure.Redis;
using HB.Infrastructure.Redis.Direct;
using HB.Infrastructure.Redis.EventBus;
using HB.Infrastructure.Redis.KVStore;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisServiceCollectionExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<RedisOptions>(configuration);

            serviceCollection.AddSingleton<IRedisConnectionManager, RedisConnectionManager>();

            serviceCollection.AddSingleton<IRedisDatabase, RedisDatabase>();

            serviceCollection.AddSingleton<IKVStoreEngine, RedisKVStoreEngine>();

            serviceCollection.AddSingleton<IEventBusEngine, RedisEventBusEngine>();

            return serviceCollection;
        }

        public static IServiceCollection AddRedis(this IServiceCollection serviceCollection, Action<RedisOptions> redisOptionsSetup)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<RedisOptions>(redisOptionsSetup);

            serviceCollection.AddSingleton<IRedisConnectionManager, RedisConnectionManager>();

            serviceCollection.AddSingleton<IRedisDatabase, RedisDatabase>();

            serviceCollection.AddSingleton<IKVStoreEngine, RedisKVStoreEngine>();

            serviceCollection.AddSingleton<IEventBusEngine, RedisEventBusEngine>();

            return serviceCollection;
        }

    }
}
