using System;
using System.Collections.Generic;
using System.Text;
using HB.Infrastructure.Aliyun.Oss;
using HB.Infrastructure.Aliyun.Sts;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AliyunStsServiceCollectionExtensions
    {
        public static IServiceCollection AddAliyunSts(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddOptions();

            services.Configure<AliyunStsOptions>(configuration);

            services.AddSingleton<IAliyunStsService, AliyunStsService>();

            return services;
        }

        public static IServiceCollection AddAliyunSts(this IServiceCollection services, Action<AliyunStsOptions> action)
        {
            //services.AddOptions();

            services.Configure(action);

            services.AddSingleton<IAliyunStsService, AliyunStsService>();

            return services;
        }
    }
}
