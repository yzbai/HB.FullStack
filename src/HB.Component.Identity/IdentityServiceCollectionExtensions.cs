using HB.Component.Identity;
using HB.Component.Identity.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection AddUserIdentity(this IServiceCollection services, Action<IdentityOptions> optionsSetup)
        {
            services.AddOptions();

            services.Configure(optionsSetup);

            services.AddSingleton<IUserBiz, UserBiz>();
            services.AddSingleton<IUserClaimBiz, UserClaimBiz>();
            services.AddSingleton<IRoleBiz, RoleBiz>();
            services.AddSingleton<IClaimsPrincipalFactory, ClaimsPrincipalFactory>();

            return services;
        }
    }
    
}
