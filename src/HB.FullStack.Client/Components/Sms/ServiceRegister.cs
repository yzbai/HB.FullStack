using HB.FullStack.Client.Components.Sms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SmsServiceRegister
    {
        public static IServiceCollection AddSmsService(this IServiceCollection services)
        {
            services.AddSingleton<ISmsService, SmsService>();

            return services;
        }
    }
}
