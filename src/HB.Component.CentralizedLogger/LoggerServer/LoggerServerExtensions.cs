using HB.Component.CentralizedLogger.LoggerServer;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LoggerServerExtensions
    {
        public static IServiceCollection AddCentralizedLoggerEventHandler(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<LoggerEventHandlerOptions>(configuration);
            services.AddEventHandler<LoggerEventHandler>();
            return services;
        }

        public static IServiceCollection AddCentralizedLoggerEventHandler(this IServiceCollection services, Action<LoggerEventHandlerOptions> action)
        {
            services.AddOptions();
            services.Configure(action);
            services.AddEventHandler<LoggerEventHandler>();
            return services;
        }
    }
}
