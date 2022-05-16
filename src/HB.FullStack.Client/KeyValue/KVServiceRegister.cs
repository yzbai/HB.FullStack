using HB.FullStack.Client.KeyValue;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KVServiceRegister
    {
        public static IServiceCollection AddKVManager(this IServiceCollection services)
        {
            services.AddSingleton<KVManager>();
            services.AddSingleton<DbSimpleLocker>();
            services.AddSingleton<KVService>();

            return services;
        }
    }
}
