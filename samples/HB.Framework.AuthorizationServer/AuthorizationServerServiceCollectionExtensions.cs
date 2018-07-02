using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HB.Framework.AuthorizationServer.Abstractions;
using HB.Framework.AuthorizationServer;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationServerServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, Action<AuthorizationServerOptions> optionsSetup)
        {
            services.AddOptions();
            services.Configure(optionsSetup);

            services.AddSingleton<ICredentialManager, CredentialManager>();
            services.AddSingleton<IJwtBuilder, JwtBuilder>();
            services.AddSingleton<IRefreshManager, RefreshManager>();
            services.AddSingleton<ISignInManager, SignInManager>();
            services.AddSingleton<ISignInTokenBiz, SignInTokenBiz>();

            return services;
        }

        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<AuthorizationServerOptions>(configuration);

            services.AddSingleton<ICredentialManager, CredentialManager>();
            services.AddSingleton<IJwtBuilder, JwtBuilder>();
            services.AddSingleton<IRefreshManager, RefreshManager>();
            services.AddSingleton<ISignInManager, SignInManager>();
            services.AddSingleton<ISignInTokenBiz, SignInTokenBiz>();

            return services;
        }
    }
}
