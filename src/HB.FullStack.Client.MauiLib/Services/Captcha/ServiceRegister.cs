using HB.FullStack.Client.MauiLib.Services.Captcha;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegisterCaptcha
    {
        public static IServiceCollection AddTCaptcha(this IServiceCollection services, string appid)
        {
            CaptchaPopup.AppId = appid;

            return services;
        }
    }
}
