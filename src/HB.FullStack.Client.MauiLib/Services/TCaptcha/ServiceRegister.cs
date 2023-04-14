using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.Client.MauiLib.Services.TCaptcha
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
