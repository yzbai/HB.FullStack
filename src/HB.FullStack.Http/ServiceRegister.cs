using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.WebApi;
using HB.FullStack.WebApi.Filters;
using HB.FullStack.WebApi.Security;
using HB.FullStack.WebApi.UserActivityTrace;

using Microsoft.Extensions.Configuration;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class FullStackServerServiceRegister
    {
        public static IServiceCollection AddServerService(this IServiceCollection services)
        {
            services.AddOptions();

            AddCore(services);

            return services;
        }

        private static void AddCore(IServiceCollection services)
        {
            //HB.FullStack.Server
            services.AddSingleton<ISecurityService, DefaultSecurityService>();
            services.AddSingleton<IPublicResourceTokenService, PublicResourceTokenService>();

            //UserActivity
            services.AddSingleton<UserActivityRepo>();
            services.AddSingleton<IUserActivityService, UserActivityService>();
            services.AddScoped<UserActivityFilter>();
            
            
            services.AddScoped<CheckPublicResourceTokenFilter>();
        }
    }
}
