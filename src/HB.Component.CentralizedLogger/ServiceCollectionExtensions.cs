using HB.Component.CentralizedLogger;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCentralizedLogger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<LoggerOptions>(configuration);

            services.AddSingleton<LoggerProcessor>();
            services.AddSingleton<LoggerProvider>();

            return services;
        }

        public static IServiceCollection AddCentralizedLogger(this IServiceCollection services, Action<LoggerOptions> configAction)
        {
            services.AddOptions();

            services.Configure(configAction);

            services.AddSingleton<LoggerProcessor>();
            services.AddSingleton<LoggerProvider>();

            return services;
        }
    }
}
