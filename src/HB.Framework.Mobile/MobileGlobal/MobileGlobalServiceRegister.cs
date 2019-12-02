using HB.Framework.Mobile;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MobileServiceRegister
    {
        public static IServiceCollection AddMobileGlobal(this IServiceCollection services)
        {
            services.AddSingleton<IMobileGlobal, MobileGlobal>();

            return services;
        }
    }
}
