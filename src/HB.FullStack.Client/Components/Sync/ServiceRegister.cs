using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.Components.Sync;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SyncManagerServiceRegister
    {
        public static IServiceCollection AddSyncManager(this IServiceCollection services)
        {
            services.AddSingleton<ISyncManager, SyncManager>();

            return services;
        }
    }
}
