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
            services.AddSingleton<UserBiz>();
            services.AddSingleton<RoleOfUserBiz>();
            services.AddSingleton<UserClaimBiz>();
            services.AddSingleton<RoleBiz>();
            services.AddSingleton<UserLoginControlBiz>();
            services.AddSingleton<SignInTokenBiz>();

            //public interface
            services.AddSingleton<IIdentityService, IdentityService>();

            return services;
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            return services.AddIdentity(o => { });
        }

        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, Action<AuthorizationServiceOptions> action)
        {
            if (!services.Any(s => s.ServiceType == typeof(IIdentityService)))
            {
                throw new FrameworkException("AuthroizationService需要IdentityService");
            }

            services.AddOptions();

            services.Configure(action);

            services.AddSingleton<IAuthorizationService, AuthorizationService>();

            return services;
        }

        public static IServiceCollection AddAuthorizationServer(this IServiceCollection services, IConfiguration configuration)
        {
            if (!services.Any(s => s.ServiceType == typeof(IIdentityService)))
            {
                throw new FrameworkException("AuthroizationService需要IdentityService");
            }

            services.AddOptions();

            services.Configure<AuthorizationServiceOptions>(configuration);

            services.AddSingleton<IAuthorizationService, AuthorizationService>();

            return services;
        }
    }

}
