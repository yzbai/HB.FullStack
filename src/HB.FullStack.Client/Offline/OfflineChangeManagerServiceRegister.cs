using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.Offline;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OfflineChangeManagerServiceRegister
    {
        public static IServiceCollection AddHistoryManager(this IServiceCollection services)
        {
            services.AddSingleton<IOfflineChangeManager, OfflineChangeManager>();

            return services;
        }
    }
}
