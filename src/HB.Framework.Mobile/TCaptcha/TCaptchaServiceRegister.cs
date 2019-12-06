using HB.Framework.Client.TCaptcha;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TCaptchaServiceRegister
    {
        public static IServiceCollection AddTCaptcha(this IServiceCollection services, Action<TCaptchaOptions> action)
        {
            services.AddOptions();

            services.Configure(action);

            return services;
        }

        public static IServiceCollection AddTCaptcha(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<TCaptchaOptions>(configuration);

            return services;
        }
    }
}
