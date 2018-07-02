using Microsoft.Extensions.Configuration;
using System;
using HB.Infrastructure.Juhe.Sms;
using HB.Framework.CommonCompnents.Sms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JuheSmsServiceCollectionExtensions
    {
        public static IServiceCollection AddJuheSms(this IServiceCollection services, Action<JuheSmsOptions> senderSetup)
        {
            services.AddOptions();

            services.Configure(senderSetup);

            services.AddSingleton<ISmsBiz, JuheSmsBiz>();

            return services;
        }

        public static IServiceCollection AddJuheSms(this IServiceCollection services, IConfiguration senderconfigure)
        {
            services.AddOptions();

            services.Configure<JuheSmsOptions>(senderconfigure);

            services.AddSingleton<ISmsBiz, JuheSmsBiz>();

            return services;
        }
    }
}
