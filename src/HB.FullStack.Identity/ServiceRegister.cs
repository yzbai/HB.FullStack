using HB.FullStack.Server.Identity;
using HB.FullStack.Server.Identity.Repos;

using Microsoft.Extensions.Configuration;

using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddIdentity<TId>(this IServiceCollection services, Action<IdentityOptions> optionsSetup)
        {
            //services.AddOptions();

            services.Configure(optionsSetup);
            AddIdentityCore<TId>(services);

            return services;
        }

        public static IServiceCollection AddIdentity<TId>(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddOptions();

            services.Configure<IdentityOptions>(configuration);

            AddIdentityCore<TId>(services);

            return services;
        }

        private static void AddIdentityCore<TId>(IServiceCollection services)
        {
            //internal
            services.AddSingleton<UserRepo<TId>>();
            services.AddSingleton<UserProfileRepo<TId>>();
            services.AddSingleton<UserClaimRepo<TId>>();
            services.AddSingleton<RoleRepo<TId>>();
            services.AddSingleton<LoginControlRepo<TId>>();
            services.AddSingleton<TokenCredentialRepo<TId>>();
            services.AddSingleton<UserActivityRepo<TId>>();

            //public interface
            services.AddSingleton<IIdentityService<TId>, IdentityService<TId>>();
        }

        public static IServiceCollection AddIdentity<TId>(this IServiceCollection services)
        {
            return services.AddIdentity<TId>(o => { });
        }
    }

}
