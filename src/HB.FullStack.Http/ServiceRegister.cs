using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Server;
using HB.FullStack.Server.File;
using HB.FullStack.Server.Filters;
using HB.FullStack.Server.Security;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FullStackServerServiceRegister
    {
        public static IServiceCollection AddServerService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<ServerOptions>(configuration);

            AddCore(services);

            return services;
        }

        public static IServiceCollection AddServerService(this IServiceCollection services, Action<ServerOptions> action)
        {
            services.AddOptions();
            services.Configure(action);

            AddCore(services);

            return services;
        }

        private static void AddCore(IServiceCollection services)
        {
            //HB.FullStack.Server
            services.AddSingleton<IFileService, DefaultFileService>();
            services.AddSingleton<ISecurityService, DefaultSecurityService>();
            services.AddSingleton<IPublicResourceTokenManager, PublicResourceTokenManager>();
            services.AddScoped<CheckPublicResourceTokenFilter>();
            services.AddScoped<CheckSmsCodeFilter>();
        }
    }
}
