using System;
using System.Net;

using HB.FullStack.Client.ApiClient;

using Microsoft.Extensions.Configuration;

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
            AddSignInReceiptSiteHttpClient(services, options);
            
            AddOtherSiteHttpClient(services, options);

            services.AddSingleton<IApiClient, DefaultApiClient>();

            //HttpClientHandler会随着HttpClient Dispose 而Dispose
            services.AddTransient<SignInReceiptRefreshHttpClientHandler>();

            static void AddSignInReceiptSiteHttpClient(IServiceCollection services, ApiClientOptions options)
            {
                //SignInReceiptSite
                IHttpClientBuilder signInReceiptHttpClientBuilder = services.AddHttpClient(options.SignInReceiptSiteSetting.GetHttpClientName(), httpClient=>
                {
#if NET5_0_OR_GREATER
                    httpClient.DefaultRequestVersion = options.SignInReceiptSiteSetting.HttpVersion;
#endif
                    httpClient.BaseAddress = options.SignInReceiptSiteSetting.BaseUrl;

                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);


                });

                if (options.ConfigureHttpMessageHandler != null)
                {
                    signInReceiptHttpClientBuilder.ConfigurePrimaryHttpMessageHandler(options.ConfigureHttpMessageHandler);
                }
            }

            static void AddOtherSiteHttpClient(IServiceCollection services, ApiClientOptions options)
            {
                //添加各站点的HttpClient
                foreach (SiteSetting endpoint in options.OtherSiteSettings)
                {
                    IHttpClientBuilder builder = services.AddHttpClient(endpoint.GetHttpClientName(), httpClient =>
                    {
#if NET5_0_OR_GREATER
                        httpClient.DefaultRequestVersion = endpoint.HttpVersion;
#endif

                        if (endpoint.BaseUrl != null)
                        {
                            httpClient.BaseAddress = endpoint.BaseUrl;
                        }

                        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
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
            }
        }
    }
}