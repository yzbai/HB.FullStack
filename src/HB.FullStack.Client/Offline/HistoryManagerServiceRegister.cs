using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.Offline;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HistoryManagerServiceRegister
    {
        public static IServiceCollection AddHistoryManager(this IServiceCollection services)
        {
            services.AddSingleton<IHistoryManager, HistoryManager>();

            return services;
        }
    }
}
