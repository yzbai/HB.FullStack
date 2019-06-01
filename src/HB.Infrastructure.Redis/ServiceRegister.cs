using HB.Framework.EventBus;
using HB.Framework.KVStore.Engine;
using HB.Infrastructure.Redis;
using HB.Infrastructure.Redis.Direct;
using HB.Infrastructure.Redis.DuplicateCheck;
using HB.Infrastructure.Redis.EventBus;
using HB.Infrastructure.Redis.KVStore;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddRedis(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<RedisOptions>(configuration);
            AddService(serviceCollection);

            return serviceCollection;
        }

        

        public static IServiceCollection AddRedis(this IServiceCollection serviceCollection, Action<RedisOptions> redisOptionsSetup)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<RedisOptions>(redisOptionsSetup);

            AddService(serviceCollection);

            return serviceCollection;
        }

        private static void AddService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IRedisInstanceManager, RedisInstanceManager>();

            serviceCollection.AddSingleton<IRedisDatabase, RedisDatabase>();

            serviceCollection.AddSingleton<IKVStoreEngine, RedisKVStoreEngine>();

            serviceCollection.AddSingleton<IEventBusEngine, RedisEventBusEngine>();
        }

    }
}
