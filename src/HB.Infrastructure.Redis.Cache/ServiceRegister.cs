using HB.FullStack.Cache;
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
            //services.AddOptions();

            services.Configure<RedisCacheOptions>(configuration);

            AddCore(services);

            return services;
        }


        public static IServiceCollection AddRedisCache(this IServiceCollection services, Action<RedisCacheOptions> action)
        {
            //services.AddOptions();

            services.Configure<RedisCacheOptions>(action);

            AddCore(services);

            return services;
        }
        private static void AddCore(IServiceCollection services)
        {
            services.AddSingleton<IModelCache, RedisCache>();

        }
    }
}
