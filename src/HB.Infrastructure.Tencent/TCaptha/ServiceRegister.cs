using HB.Infrastructure.Tencent;
using HB.Infrastructure.Tencent.TCaptha;

using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddTCaptha(this IServiceCollection services, Action<TCapthaOptions> action)
        {

            //services.AddOptions();

            services.Configure(action);

            TCapthaOptions options = new TCapthaOptions();
            action(options);

            AddTCapthaCore(services);

            return services;
        }

        public static IServiceCollection AddTCaptha(this IServiceCollection services, IConfiguration configuration)
        {

            //services.AddOptions();

            services.Configure<TCapthaOptions>(configuration);

            TCapthaOptions options = new TCapthaOptions();
            configuration.Bind(options);

            AddTCapthaCore(services);

            return services;
        }
        
        private static void AddTCapthaCore(IServiceCollection services)
        {
            services.AddHttpClient(TCapthaOptions.ENDPOINT_NAME, httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(ITCapthaClient).FullName);
            });

            services.AddSingleton<ITCapthaClient, TCapthaClient>();

            services.AddScoped<TCapthcaCheckFilter>();
        }

    }
}
