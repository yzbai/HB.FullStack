/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Client.MauiLib.Components;

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