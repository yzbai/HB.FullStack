using HB.Framework.EventBus;
using HB.Infrastructure.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KafkaEngineServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaEngine(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<KafkaEngineOptions>(configuration);

            services.AddSingleton<IEventBusEngine, KafkaEngine>();

            return services;
        }

        public static IServiceCollection AddKafkaEngine(this IServiceCollection services, Action<KafkaEngineOptions> action)
        {
            services.AddOptions();

            services.Configure(action);

            services.AddSingleton<IEventBusEngine, KafkaEngine>();

            return services;
        }
    }
}
