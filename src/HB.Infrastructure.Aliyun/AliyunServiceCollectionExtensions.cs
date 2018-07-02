using Aliyun.Acs.Core.Profile;
using HB.Infrastructure.Aliyun;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Aliyun.Acs.Core;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AliyunServiceCollectionExtensions
    {
        public static IServiceCollection AddAliyunClient(this IServiceCollection services, IConfiguration configuration)
        {
            AliyunClientOptoins options = new AliyunClientOptoins();
            configuration.Bind(options);

            return services.AddAliyunClient(options);
        }

        public static IServiceCollection AddAliyunClient(this IServiceCollection services, Action<AliyunClientOptoins> optionSetup)
        {
            AliyunClientOptoins options = new AliyunClientOptoins();
            optionSetup(options);

            return services.AddAliyunClient(options);
        }

        public static IServiceCollection AddAliyunClient(this IServiceCollection services, AliyunClientOptoins options)
        {
            DefaultProfile defaultProfile = DefaultProfile.GetProfile(regionId, accessKeyId, accessKeySecret);
            DefaultProfile.AddEndpoint(regionId, regionId, "Dysmsapi", "dysmsapi.aliyuncs.com");
            DefaultProfile.AddEndpoint(regionId, regionId, "Sts", "sts.cn-hangzhou.aliyuncs.com");

            services.AddSingleton<IClientProfile>(defaultProfile);
            services.AddSingleton<IAcsClient, DefaultAcsClient>();

            return services;
        }
    }
}
