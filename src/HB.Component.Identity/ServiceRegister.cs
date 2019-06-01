using HB.Component.Identity;
using HB.Component.Identity.Abstractions;
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
            services.AddSingleton<IUserBiz, UserBiz>();
            services.AddSingleton<IUserClaimBiz, UserClaimBiz>();
            services.AddSingleton<IRoleBiz, RoleBiz>();
            services.AddSingleton<IClaimsPrincipalFactory, ClaimsPrincipalFactory>();

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
