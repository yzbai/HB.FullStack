﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.Offline;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddOfflineManager(this IServiceCollection services)
        {
            services.AddSingleton<IOfflineManager, OfflineManager>();

            return services;
        }
    }
}