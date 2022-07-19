using HB.FullStack.WebApi;
using HB.FullStack.WebApi.Filters;
using HB.FullStack.WebApi.Security;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class FullStackServerServiceRegister
    {
        public static IServiceCollection AddWebApiServerService(this IServiceCollection services, bool addAuthentication = true)
        {
            //services.AddOptions();

            AddCore(services, addAuthentication);

            return services;
        }

        private static void AddCore(IServiceCollection services, bool addAuthentication)
        {
            //HB.FullStack.WebApi
            services.AddSingleton<ISecurityService, DefaultSecurityService>();
            services.AddSingleton<ICommonResourceTokenService, CommonResourceTokenService>();

            if (addAuthentication)//(services.Where(d => d.ServiceType == typeof(IIdentityService)).Any())
            {
                services.AddScoped<UserActivityFilter>();
            }

            services.AddScoped<CheckCommonResourceTokenFilter>();
        }
    }
}
