﻿using HB.FullStack.Client;
using HB.FullStack.Client.UI.Maui;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NavigationServiceRegister
    {
        public static IServiceCollection AddNavigationManager(this IServiceCollection services)
        {
            services.AddSingleton<INavigationManager, MauiNavigationManager>();
            return services;
        }
    }
}
