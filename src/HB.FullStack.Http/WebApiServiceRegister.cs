using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Identity;
using HB.FullStack.WebApi;
using HB.FullStack.WebApi.Filters;
using HB.FullStack.WebApi.Security;

using Microsoft.Extensions.Configuration;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class FullStackServerServiceRegister
    {
        public static IServiceCollection AddWebApiServerService(this IServiceCollection services)
        {
            //services.AddOptions();

            AddCore(services);

            return services;
        }

        private static void AddCore(IServiceCollection services)
        {
            //HB.FullStack.WebApi
            services.AddSingleton<ISecurityService, DefaultSecurityService>();
            services.AddSingleton<ICommonResourceTokenService, CommonResourceTokenService>();

            if (services.Where(d => d.ServiceType == typeof(IIdentityService)).Any())
            {
                services.AddScoped<UserActivityFilter>();
            }

            services.AddScoped<CheckCommonResourceTokenFilter>();
        }
    }
}
