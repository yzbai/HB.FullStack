using HB.Framework.KVStore.Engine;
using HB.Infrastructure.Redis.KVStore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KVStoreServiceRegister
    {
        public static IServiceCollection AddRedisKVStore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<RedisKVStoreOptions>(configuration);

            services.AddSingleton<IKVStoreEngine, RedisKVStoreEngine>();

            services.AddKVStore();

            return services;
        }

        public static IServiceCollection AddRedisKVStore(this IServiceCollection services, Action<RedisKVStoreOptions> action)
        {
            services.AddOptions();
            services.Configure(action);

            services.AddSingleton<IKVStoreEngine, RedisKVStoreEngine>();

            services.AddKVStore();

            return services;
        }
    }
}
