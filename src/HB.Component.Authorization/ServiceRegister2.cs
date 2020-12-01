using HB.Component.Authorization;
using HB.Component.Authorization.Abstractions;
using HB.Component.Identity;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationServerServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, Action<AuthorizationServerOptions> optionsSetup)
        {
            if (!services.Any(sd => sd.ServiceType == typeof(IIdentityService)))
            {
                throw new FrameworkException($"AuthorizationService需要IIdentityService");
            }

            services.AddOptions();
            services.Configure(optionsSetup);

            AddService(services);

            return services;
        }

        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, IConfiguration configuration)
        {
            if (!services.Any(sd => sd.ServiceType == typeof(IIdentityService)))
            {
                throw new FrameworkException($"AuthorizationService需要IIdentityService");
            }

            services.AddOptions();
            services.Configure<AuthorizationServerOptions>(configuration);

            AddService(services);

            return services;
        }

        private static void AddService(IServiceCollection services)
        {
            //internal
            services.AddSingleton<CredentialBiz>();
            services.AddSingleton<JwtBuilder>();
            services.AddSingleton<SignInTokenBiz>();

            //public interface
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
        }
    }
}
