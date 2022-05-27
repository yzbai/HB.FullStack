using HB.FullStack.Client.Network;
using HB.FullStack.Client.Maui.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NetworkServiceRegister
    {
        public static IServiceCollection AddNetworkManager(this IServiceCollection services)
        {
            services.AddSingleton<ConnectivityManager, MauiConnectivityManager>();

            return services;
        }
    }
}
