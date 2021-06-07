using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.XamarinForms.TCaptcha;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegisterTCaptcha
    {
        public static IServiceCollection AddTCaptcha(this IServiceCollection services, string appid)
        {
            TCaptchaDialog.AppId = appid;

            return services;
        }
    }
}
