using Microsoft.Extensions.Configuration;

using System;
using System.Net.Http;
using System.Linq;
using HB.FullStack.Common.ApiClient;
using System.Net;

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

        private static void AddApiClientCore(IServiceCollection services, ApiClientOptions options)
        {
            //添加默认HttpClient
            services.AddHttpClient(ApiClientOptions.NO_BASEURL_HTTPCLIENT_NAME, httpClient =>
            {
#if NET5_0_OR_GREATER
                httpClient.DefaultRequestVersion = HttpVersion.Version20;
#endif
                httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(DefaultApiClient).FullName);
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    {
                        if (cert!.Issuer.Equals("CN=localhost", GlobalSettings.Comparison))
                            return true;
                        return errors == System.Net.Security.SslPolicyErrors.None;
                    }
                };
                return handler;
            })
#endif
            ;

            //添加各站点的HttpClient
            foreach (EndpointSettings endpoint in options.Endpoints)
            {
                services.AddHttpClient(endpoint.HttpClientName, httpClient =>
                {
#if NET5_0_OR_GREATER
                    httpClient.DefaultRequestVersion = HttpVersion.Version20;
#endif
                    httpClient.BaseAddress = endpoint.Url;
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(DefaultApiClient).FullName);
                })

                //TODO: 调查这个
                //.AddTransientHttpErrorPolicy(p =>
                //{
                //    //TODO: Move this to options
                //    return p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(1000));
                //})
#if DEBUG
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    HttpClientHandler handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                        {
                            if (cert!.Issuer.Equals("CN=localhost", GlobalSettings.Comparison))
                                return true;
                            return errors == System.Net.Security.SslPolicyErrors.None;
                        }
                    };
                    return handler;
                })
#endif
                ;
            }

            services.AddSingleton<IApiClient, DefaultApiClient>();

            //HttpClientHandler会随着HttpClient Dispose 而Dispose
            services.AddTransient<TokenAutoRefreshedHttpClientHandler>();
        }
    }
}