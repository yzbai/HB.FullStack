using HB.Framework.DistributedQueue;
using HB.Framework.KVStore;
using HB.Framework.KVStore.Engine;
using HB.Infrastructure.Redis;
using HB.Infrastructure.Redis.KVStore;
using HB.Infrastructure.Redis.Queue;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisKVStoreServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisEngine(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<RedisEngineOptions>(configuration);

            serviceCollection.AddSingleton<IKVStoreEngine, RedisKVStoreEngine>();

            serviceCollection.AddSingleton<IDistributedQueue, RedisQueue>();

            return serviceCollection;
        }

        public static IServiceCollection AddRedisEngine(this IServiceCollection serviceCollection, Action<RedisEngineOptions> redisEngineOptionsSetup)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<RedisEngineOptions>(redisEngineOptionsSetup);

            serviceCollection.AddSingleton<IKVStoreEngine, RedisKVStoreEngine>();

            serviceCollection.AddSingleton<IDistributedQueue, RedisQueue>();

            return serviceCollection;
        }

    }
}
