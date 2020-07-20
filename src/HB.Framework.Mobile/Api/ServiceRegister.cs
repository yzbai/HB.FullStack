using HB.Framework.Client.Api;
using Microsoft.Extensions.Configuration;
using Polly;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddApiClient(this IServiceCollection services, Action<ApiClientOptions> action)
        {
            services.Configure(action);

            ApiClientOptions options = new ApiClientOptions();
            action(options);

            AddApiClientCore(services, options);

            return services;
        }

        public static IServiceCollection AddApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiClientOptions>(configuration);

            ApiClientOptions options = new ApiClientOptions();
            configuration.Bind(options);

            AddApiClientCore(services, options);

            return services;
        }

        /// <summary>
        /// AddApiClientCore
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        private static void AddApiClientCore(IServiceCollection services, ApiClientOptions options)
        {
            options.Endpoints.ForEach(endpoint =>
            {
                services.AddHttpClient(EndpointSettings.GetHttpClientName(endpoint), httpClient =>
                {
                    httpClient.BaseAddress = endpoint.Url;
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(ApiClient).FullName);
                })
                .AddTransientHttpErrorPolicy(p =>
                {
                    //TODO: Move this to options
                    return p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600));
                })
#if DEBUG
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    HttpClientHandler handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    {
                        if (cert.Issuer.Equals("CN=localhost", GlobalSettings.Comparison))
                            return true;
                        return errors == System.Net.Security.SslPolicyErrors.None;
                    };
                    return handler;
                })
#endif
                ;
            });



            services.AddSingleton<IApiClient, ApiClient>();
        }

    }
}
