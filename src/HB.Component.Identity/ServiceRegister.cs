using HB.Component.Identity;
using HB.Component.Identity.Biz;

using System;

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
            services.AddSingleton<ClaimsPrincipalFactory>();
            services.AddSingleton<UserLoginControlBiz>();

            //public interface
            services.AddSingleton<IIdentityService, IdentityService>();

            return services;
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            return services.AddIdentity(o => { });
        }
    }

}
