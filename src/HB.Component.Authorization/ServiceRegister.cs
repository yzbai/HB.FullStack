using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationServerServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, Action<AuthorizationOptions> optionsSetup)
        {
            services.AddOptions();
            services.Configure(optionsSetup);

            AddService(services);

            return services;
        }

        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<AuthorizationOptions>(configuration);

            AddService(services);

            return services;
        }

        private static void AddService(IServiceCollection services)
        {
            //internal
            services.AddSingleton<ICredentialBiz, CredentialBiz>();
            services.AddSingleton<IJwtBuilder, JwtBuilder>();
            services.AddSingleton<ISignInTokenBiz, SignInTokenBiz>();

            //public interface
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
        }
    }
}
