using System;
using HB.FullStack.Server.Services;
using HB.Infrastructure.Aliyun.Sms;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AliyunSmsServiceCollectionExtensions
    {
        public static IServiceCollection AddAliyunSms(this IServiceCollection services, Action<AliyunSmsOptions> optionSetup)
        {
            //services.AddOptions();
            services.Configure(optionSetup);

            services.AddSingleton<ISmsServerService, AliyunSmsService>();

            return services;
        }

        public static IServiceCollection AddAliyunSms(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddOptions();
            services.Configure<AliyunSmsOptions>(configuration);

            services.AddSingleton<ISmsServerService, AliyunSmsService>();

            return services;
        }
    }
}
