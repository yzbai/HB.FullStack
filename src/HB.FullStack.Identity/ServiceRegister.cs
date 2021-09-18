using HB.FullStack.Identity;

using Microsoft.Extensions.Configuration;

using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddIdentity(this IServiceCollection services, Action<IdentityOptions> optionsSetup)
        {
            services.AddOptions();

            services.Configure(optionsSetup);

            //internal
            services.AddSingleton<UserRepo>();
            services.AddSingleton<UserRoleRepo>();
            services.AddSingleton<UserClaimRepo>();
            services.AddSingleton<RoleRepo>();
            services.AddSingleton<UserLoginControlRepo>();
            services.AddSingleton<SignInTokenRepo>();

            //public interface
            services.AddSingleton<IIdentityService, IdentityService>();

            return services;
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            return services.AddIdentity(o => { });
        }

        /// <summary>
        /// AddAuthorizationServer
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="IdentityException"></exception>
        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, Action<AuthorizationServiceOptions> action)
        {
            ThrowIfNoIdentityService(services);

            services.AddOptions();

            services.Configure(action);

            services.AddSingleton<IAuthorizationService, AuthorizationService>();

            return services;
        }

        

        /// <summary>
        /// AddAuthorizationServer
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="IdentityException"></exception>
        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, IConfiguration configuration)
        {
            ThrowIfNoIdentityService(services);

            services.AddOptions();

            services.Configure<AuthorizationServiceOptions>(configuration);

            services.AddSingleton<IAuthorizationService, AuthorizationService>();

            return services;
        }

        /// <summary>
        /// ThrowIfNoIdentityService
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="IdentityException"></exception>
        private static void ThrowIfNoIdentityService(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IIdentityService)))
            {
                throw Exceptions.ServiceRegisterError(cause:"AuthroizationService需要IdentityService");
            }
        }
    }

}
