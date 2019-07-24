using HB.Infrastructure.Tencent;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddTCaptha(this IServiceCollection services, Action<TCapthaOptions> action)
        {
            ThrowIf.Null(action, nameof(action));

            services.AddOptions();

            services.Configure(action);

            TCapthaOptions options = new TCapthaOptions();
            action(options);

            AddTCapthaCore(services, options);

            return services;
        }

        public static IServiceCollection AddTCaptha(this IServiceCollection services, IConfiguration configuration)
        {
            ThrowIf.Null(configuration, nameof(configuration));

            services.AddOptions();

            services.Configure<TCapthaOptions>(configuration);

            TCapthaOptions options = new TCapthaOptions();
            configuration.Bind(options);

            AddTCapthaCore(services, options);

            return services;
        }

        private static void AddTCapthaCore(IServiceCollection services, TCapthaOptions options)
        {
            services.AddHttpClient(TCapthaOptions.EndpointName, httpClient => {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(ITCapthaClient).FullName);
            });

            services.AddSingleton<ITCapthaClient, TCapthaClient>();
        }

    }
}
