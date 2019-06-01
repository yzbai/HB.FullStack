using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, Action<EventBusOptions> action)
        {
            services.AddOptions();
            services.Configure(action);
            services.AddSingleton<IEventBus, DefaultEventBus>();

            return services;
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<EventBusOptions>(configuration);
            services.AddSingleton<IEventBus, DefaultEventBus>();

            return services;
        }

        //public static IServiceCollection AddEventHandler<T>(this IServiceCollection services) where T: IEventHandler
        //{
        //    services.Add(new ServiceDescriptor(typeof(IEventHandler), typeof(T), ServiceLifetime.Singleton));
        //    return services;
        //}
    }
}
