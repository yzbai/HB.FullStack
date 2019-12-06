using HB.Framework.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClientGlobalServiceRegister
    {
        public static IServiceCollection AddClientGlobal(this IServiceCollection services)
        {
            services.AddSingleton<IClientGlobal, ClientGlobal>();

            return services;
        }
    }
}
