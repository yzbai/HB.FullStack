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

            services.Configure<CentralizedLoggerOptions>(configuration);

            services.AddSingleton<CentralizedLoggerProcessor>();
            services.AddSingleton<CentralizedLoggerProvider>();

            return services;
        }

        public static IServiceCollection AddCentralizedLogger(this IServiceCollection services, Action<CentralizedLoggerOptions> configAction)
        {
            services.AddOptions();

            services.Configure(configAction);

            services.AddSingleton<CentralizedLoggerProcessor>();
            services.AddSingleton<CentralizedLoggerProvider>();

            return services;
        }
    }
}
