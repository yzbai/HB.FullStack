using HB.FullStack.EventBus;
using HB.FullStack.EventBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            //services.AddOptions();

            services.AddSingleton<IEventBus, DefaultEventBus>();

            return services;
        }
    }
}
