using HB.Framework.EventBus;
using HB.Infrastructure.Redis.EventBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisEventBusServiceRegister
    {
        public static IServiceCollection AddRedisEventBus(this IServiceCollection services, Action<RedisEventBusOptions> action)
        {
            services.AddOptions();

            services.Configure(action);

            services.AddSingleton<IEventBusEngine, RedisEventBusEngine>();

            services.AddEventBus();

            return services;
        }

        public static IServiceCollection AddRedisEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<RedisEventBusOptions>(configuration);

            services.AddSingleton<IEventBusEngine, RedisEventBusEngine>();

            services.AddEventBus();

            return services;
        }
    }
}
