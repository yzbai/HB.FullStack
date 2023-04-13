using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.Client.Services.Offline
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddOfflineManager(this IServiceCollection services)
        {
            services.AddSingleton<ISyncManager, SyncManager>();

            return services;
        }
    }
}
