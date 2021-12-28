using System;
using System.Collections.Generic;
using System.Text;
using HB.Infrastructure.Aliyun.Oss;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AliyunOssServiceCollectionExtensions
    {
        public static IServiceCollection AddAliyunOss(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddOptions();

            services.Configure<AliyunOssOptions>(configuration);

            services.AddSingleton<IAliyunOssService, AliyunOssService>();

            return services;
        }

        public static IServiceCollection AddAliyunOss(this IServiceCollection services, Action<AliyunOssOptions> action)
        {
            //services.AddOptions();

            services.Configure(action);

            services.AddSingleton<IAliyunOssService, AliyunOssService>();

            return services;
        }
    }
}
