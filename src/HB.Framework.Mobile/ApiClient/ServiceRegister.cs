using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Http;
using Polly;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using HB.Framework.Mobile.ApiClient;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddApiClient(this IServiceCollection services, Action<ApiClientOptions> action)
        {
            ThrowIf.Null(action, nameof(action));

            services.Configure(action);

            ApiClientOptions options = new ApiClientOptions();
            action(options);

            AddApiClientCore(services, options);

            return services;
        }

        public static IServiceCollection AddApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            ThrowIf.Null(configuration, nameof(configuration));

            services.Configure<ApiClientOptions>(configuration);

            ApiClientOptions options = new ApiClientOptions();
            configuration.Bind(options);

            AddApiClientCore(services, options);

            return services;
        }

        private static void AddApiClientCore(IServiceCollection services, ApiClientOptions options)
        {
            options.Endpoints.ForEach(endpoint => {
                services.AddHttpClient(EndpointSettings.GetHttpClientName(endpoint), httpClient => {
                    httpClient.BaseAddress = new Uri(endpoint.Url);
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(ApiClient).FullName);
                })
                .AddTransientHttpErrorPolicy(p => {
                    //TODO: Move this to options
                    return p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600));
                });
            });



            services.AddSingleton<IApiClient, ApiClient>();
        }

    }
}
