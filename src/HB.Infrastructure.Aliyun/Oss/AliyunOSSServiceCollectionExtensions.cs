using System;
using System.Collections.Generic;
using System.Text;
using HB.Infrastructure.Aliyun.Oss;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AliyunOssServiceCollectionExtensions
    {
        public IServiceCollection AddAliyunOss(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<AliyunOssOptions>(configuration);
            return services;
        }

        public IServiceCollection AddAliyunOss(this IServiceCollection services, Action<AliyunOssOptions> action)
        {
            services.AddOptions();

            services.Configure(action);


            return services;
        }
    }
}
