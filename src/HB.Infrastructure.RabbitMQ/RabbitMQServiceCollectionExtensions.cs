using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.EventBus;
using HB.Infrastructure.RabbitMQ;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQEngine(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<RabbitMQEngineOptions>(configuration);

            services.AddSingleton<IEventBusEngine, RabbitMQEventBusEngine>();

            return services;
        }
    }
}
