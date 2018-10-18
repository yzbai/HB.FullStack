using HB.Component.Resource.Vod;
using HB.Infrastructure.Aliyun.Vod;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AliyunVodServiceCollectionExtensions
    {
        public static IServiceCollection AddAliyunVod(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<AliyunVodOptions>(configuration);

            services.AddSingleton<IVodService, AliyunVodService>();

            return services;
        }

        public static IServiceCollection AddAliyunVod(this IServiceCollection services, Action<AliyunVodOptions> action)
        {
            services.AddOptions();
            services.Configure(action);
            services.AddSingleton<IVodService, AliyunVodService>();

            return services;
        }
    }

    
}
