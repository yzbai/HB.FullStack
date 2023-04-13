using HB.FullStack.Client.Services.DbLocker;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Services.KeyValue
{
    public static class KVServiceRegister
    {
        public static IServiceCollection AddKVManager(this IServiceCollection services)
        {
            services.AddSingleton<KVRepo>();
            services.AddSingleton<DbSimpleLocker>();
            services.AddSingleton<KVService>();

            return services;
        }
    }
}
