using HB.Component.CentralizedLogger.LoggerServer;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CentralizedLoggerServerExtensions
    {
        public static IServiceCollection AddCentralizedLoggerEventHandler(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<CentralizedLoggerEventHandlerOptions>(configuration);
            services.AddEventHandler<CentralizedLoggerEventHandler>();
            return services;
        }

        public static IServiceCollection AddCentralizedLoggerEventHandler(this IServiceCollection services, Action<CentralizedLoggerEventHandlerOptions> action)
        {
            services.AddOptions();
            services.Configure(action);
            services.AddEventHandler<CentralizedLoggerEventHandler>();
            return services;
        }
    }
}
