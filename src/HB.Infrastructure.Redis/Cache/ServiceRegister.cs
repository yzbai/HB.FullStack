using HB.Framework.Cache;
using HB.Infrastructure.Redis.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisCacheServiceRegister
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<RedisCacheOptions>(configuration);

            services.AddSingleton<ICache, RedisCache>();

            services.AddSingleton<IDistributedCache>(provider => provider.GetRequiredService<ICache>());

            return services;
        }

        public static IServiceCollection AddRedisCache(this IServiceCollection services, Action<RedisCacheOptions> action)
        {
            services.AddOptions();

            services.Configure<RedisCacheOptions>(action);

            services.AddSingleton<ICache, RedisCache>();

            services.AddSingleton<IDistributedCache>(provider => provider.GetRequiredService<ICache>());

            return services;
        }
    }
}
