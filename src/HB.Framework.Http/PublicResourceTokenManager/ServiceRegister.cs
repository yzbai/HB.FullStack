﻿using HB.Framework.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddPublicResourceTokenManager(this IServiceCollection services)
        {
            services.AddSingleton<IPublicResourceTokenManager, PublicResourceTokenManager>();

            return services;
        }
    }
}
