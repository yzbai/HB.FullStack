using System;

using HB.FullStack.Common.Cache;
using HB.Infrastructure.Redis.Cache;

using Microsoft.Extensions.Configuration;

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
            services.AddSingleton<ICache, RedisCache>();

        }
    }
}
