using HB.FullStack.Client.Maui.TCaptcha;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegisterTCaptcha
    {
        public static IServiceCollection AddTCaptcha(this IServiceCollection services, string appid)
        {
            TCaptchaPopup.AppId = appid;

            return services;
        }
    }
}
