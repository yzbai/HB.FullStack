using HB.Infrastructure.Redis.Direct;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisDatabaseServiceRegister
    {
        public static IServiceCollection AddRedisDatabase(this IServiceCollection services, Action<RedisDatabaseOptions> action)
        {
            services.AddOptions();

            services.Configure(action);

            services.AddSingleton<IRedisDatabase, RedisDatabase>();

            return services;
        }

        public static IServiceCollection AddRedisDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<RedisDatabaseOptions>(configuration);

            services.AddSingleton<IRedisDatabase, RedisDatabase>();

            return services;
        }
    }
}
