using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using HB.FullStack.Mobile.IdBarriers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegisterIdBarrier
    {
        public static IServiceCollection AddIdBarrier(this IServiceCollection services)
        {
            services.AddSingleton<IIdBarrierRepo, IdBarrierRepo>();
            services.AddSingleton<IIdBarrierService, IdBarrierService>();

            return services;
        }
    }
}
