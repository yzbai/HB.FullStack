using HB.FullStack.Common.Shared.Sms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SmsClientServiceRegister
    {
        public static IServiceCollection AddSmsClientService(this IServiceCollection services)
        {
            services.AddSingleton<ISmsClientService, SmsClientService>();

            return services;
        }
    }
}
