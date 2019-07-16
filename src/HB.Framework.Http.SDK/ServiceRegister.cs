using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Http;
using Polly;

namespace HB.Framework.Http.SDK
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddResourceClient(this IServiceCollection services, Action<ResourceClientOptions> action)
        {
            ThrowIf.Null(action, nameof(action));

            bool existLocalStorage = false;
            bool existDeviceInfoProvider = false;
            Type localStorageType = typeof(ILocalStorage);
            Type deviceInfoProviderType = typeof(IDeviceInfoProvider);

            foreach (ServiceDescriptor service in services.ThrowIfNull(nameof(services)))
            {
                if (service.ServiceType == localStorageType)
                {
                    existLocalStorage = true;
                }
                else if (service.ServiceType == deviceInfoProviderType)
                {
                    existDeviceInfoProvider = true;
                }

                if (existDeviceInfoProvider && existLocalStorage)
                {
                    break;
                }
            }

            if (!existLocalStorage)
            {
                throw new ArgumentException("ResourceClient need ILocalStorage Service.");
            }

            if (!existDeviceInfoProvider)
            {
                throw new ArgumentException("ResourceClient need IDeviceInfoProvider Service.");
            }

            ResourceClientOptions options = new ResourceClientOptions();

            action(options);

            options.Endpoints.ForEach(kv => {
                services.AddHttpClient(kv.Key.ToString(GlobalSettings.Culture), httpClient => {
                    httpClient.BaseAddress = kv.Value;
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(ResourceClient).FullName);
                }).AddTransientHttpErrorPolicy(p=> {
                    return p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600));
                });
            });

            services.Configure(action);


            services.AddSingleton<IResourceClient, ResourceClient>();

 
            return services;
        }
    }
}
