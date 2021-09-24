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
            AddIdentityCore(services);

            return services;
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<IdentityOptions>(configuration);

            AddIdentityCore(services);

            return services;
        }

        private static void AddIdentityCore(IServiceCollection services)
        {
            //internal
            services.AddSingleton<UserEntityRepo>();
            services.AddSingleton<UserRoleEntityRepo>();
            services.AddSingleton<UserClaimEntityRepo>();
            services.AddSingleton<RoleEntityRepo>();
            services.AddSingleton<LoginControlEntityRepo>();
            services.AddSingleton<SignInTokenEntityRepo>();
            services.AddSingleton<UserActivityEntityRepo>();

            //public interface
            services.AddSingleton<IIdentityService, IdentityService>();
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            return services.AddIdentity(o => { });
        }
    }

}
