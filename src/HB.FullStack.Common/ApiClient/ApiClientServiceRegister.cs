using Microsoft.Extensions.Configuration;

using System;
using System.Net.Http;
using System.Linq;
using HB.FullStack.Common.ApiClient;
using System.Net;
using System.Globalization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiClientServiceRegister
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

        private static void AddApiClientCore(IServiceCollection services, ApiClientOptions options)
        {
            //添加默认HttpClient
            IHttpClientBuilder httpClientBuilder = services.AddHttpClient(ApiClientOptions.NO_BASEURL_HTTPCLIENT_NAME, httpClient =>
            {
#if NET5_0_OR_GREATER
                httpClient.DefaultRequestVersion = HttpVersion.Version20;
#endif
                httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(DefaultApiClient).FullName);
            });

            if (options.ConfigureHttpMessageHandler != null)
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(options.ConfigureHttpMessageHandler);
            }

            //添加各站点的HttpClient
            foreach (EndpointSettings endpoint in options.Endpoints)
            {
                IHttpClientBuilder builder = services.AddHttpClient(endpoint.HttpClientName, httpClient =>
                {
#if NET5_0_OR_GREATER
                    httpClient.DefaultRequestVersion = HttpVersion.Version20;
#endif
                    if (endpoint.BaseUrl != null)
                    {
                        httpClient.BaseAddress = endpoint.BaseUrl;
                    }

                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(DefaultApiClient).FullName);
                });

                //TODO: 调查这个
                //.AddTransientHttpErrorPolicy(p =>
                //{
                //    //TODO: Move this to options
                //    return p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(1000));
                //})

                if (options.ConfigureHttpMessageHandler != null)
                {
                    builder.ConfigurePrimaryHttpMessageHandler(options.ConfigureHttpMessageHandler);
                }
            }

            services.AddSingleton<IApiClient, DefaultApiClient>();

            //HttpClientHandler会随着HttpClient Dispose 而Dispose
            services.AddTransient<TokenAutoRefreshedHttpClientHandler>();
        }
    }
}