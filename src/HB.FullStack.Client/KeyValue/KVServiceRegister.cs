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
        public static IServiceCollection AddKV(this IServiceCollection services)
        {
            services.AddSingleton<KVRepo>();
            services.AddSingleton<DbSimpleLocker>();
            services.AddSingleton<KVService>();

            return services;
        }
    }
}
