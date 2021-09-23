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
            services.AddSingleton<UserEntityRepo>();
            services.AddSingleton<UserRoleEntityRepo>();
            services.AddSingleton<UserClaimEntityRepo>();
            services.AddSingleton<RoleEntityRepo>();
            services.AddSingleton<LoginControlEntityRepo>();
            services.AddSingleton<SignInTokenEntityRepo>();

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
