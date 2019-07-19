using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Http;
using Polly;
using System.Net.Http;

namespace HB.Framework.Http.SDK
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddResourceClient(this IServiceCollection services, Action<ResourceClientOptions> action)
        {
            ThrowIf.Null(action, nameof(action));

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

            ResourceClientOptions options = new ResourceClientOptions();

            action(options);

            options.Endpoints.ForEach(kv => {
                services.AddHttpClient(kv.Key.ToString(GlobalSettings.Culture), httpClient => {
                    httpClient.BaseAddress = kv.Value;
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(ResourceClient).FullName);
                })
                .AddTransientHttpErrorPolicy(p => {
                    //TODO: Move this to options
                    return p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600));
                });
            });

            services.Configure(action);


            services.AddSingleton<IResourceClient, ResourceClient>();

 
            return services;
        }
    }
}
