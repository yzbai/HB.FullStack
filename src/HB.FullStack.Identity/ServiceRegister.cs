using HB.FullStack.Server.Identity;
using HB.FullStack.Server.Identity.Repos;

using Microsoft.Extensions.Configuration;

using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddIdentity(this IServiceCollection services, Action<IdentityOptions> optionsSetup)
        {
            //services.AddOptions();

            services.Configure(optionsSetup);
            AddIdentityCore(services);

            return services;
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddOptions();

            services.Configure<IdentityOptions>(configuration);

            AddIdentityCore(services);

            return services;
        }

        private static void AddIdentityCore(IServiceCollection services)
        {
            //internal
            services.AddSingleton<UserRepo>();
            services.AddSingleton<UserProfileRepo>();
            services.AddSingleton<UserClaimRepo>();
            services.AddSingleton<RoleRepo>();
            services.AddSingleton<LoginControlRepo>();
            services.AddSingleton<SignInCredentialRepo>();
            services.AddSingleton<UserActivityRepo>();

            //public interface
            services.AddSingleton<IIdentityService, IdentityService>();
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            return services.AddIdentity(o => { });
        }
    }

}
