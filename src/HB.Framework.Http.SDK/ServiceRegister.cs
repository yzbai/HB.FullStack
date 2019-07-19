using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Http;
using Polly;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace HB.Framework.Http.SDK
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddResourceClient(this IServiceCollection services, Action<ResourceClientOptions> action)
        {
            ThrowIf.Null(action, nameof(action));
            CheckMobileInfoProviderExists(services);

            services.Configure(action);

            ResourceClientOptions options = new ResourceClientOptions();
            action(options);

            AddResourceClientCore(services, options);

            return services;
        }

        public static IServiceCollection AddResourceClient(this IServiceCollection services, IConfiguration configuration)
        {
            ThrowIf.Null(configuration, nameof(configuration));
            CheckMobileInfoProviderExists(services);

            services.Configure<ResourceClientOptions>(configuration);

            ResourceClientOptions options = new ResourceClientOptions();
            configuration.Bind(options);

            AddResourceClientCore(services, options);

            return services;
        }

        private static void AddResourceClientCore(IServiceCollection services, ResourceClientOptions options)
        {
            options.Endpoints.ForEach(endpoint => {
                services.AddHttpClient(Endpoint.GetHttpClientName(endpoint), httpClient => {
                    httpClient.BaseAddress = new Uri(endpoint.Url);
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(ResourceClient).FullName);
                })
                .AddTransientHttpErrorPolicy(p => {
                    //TODO: Move this to options
                    return p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600));
                });
            });



            services.AddSingleton<IResourceClient, ResourceClient>();
        }

        private static void CheckMobileInfoProviderExists(IServiceCollection services)
        {
            bool existDeviceInfoProvider = false;
            Type deviceInfoProviderType = typeof(IMobileInfoProvider);

            foreach (ServiceDescriptor service in services.ThrowIfNull(nameof(services)))
            {
                if (service.ServiceType == deviceInfoProviderType)
                {
                    existDeviceInfoProvider = true;
                    break;
                }
            }

            if (!existDeviceInfoProvider)
            {
                throw new ArgumentException("ResourceClient need IDeviceInfoProvider Service.");
            }
        }
    }
}
