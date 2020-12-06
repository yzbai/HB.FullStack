using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Lock;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryLockManagerServiceRegister
    {
        public static IServiceCollection AddMemoryLock(this IServiceCollection services, IConfiguration configuration)
        {
            if (!services.Any(sd => sd.ServiceType == typeof(IMemoryCache)))
            {
                throw new FrameworkException($"MemoryLockManager需要MemoryCache服务");
            }

            services.Configure<MemoryLockOptions>(configuration);

            services.AddSingleton<IMemoryLockManager, MemoryLockManager>();

            return services;
        }

        public static IServiceCollection AddMemoryLock(this IServiceCollection services, Action<MemoryLockOptions> action)
        {
            if (!services.Any(sd => sd.ServiceType == typeof(IMemoryCache)))
            {
                throw new FrameworkException($"MemoryLockManager需要MemoryCache服务");
            }

            services.Configure(action);

            services.AddSingleton<IMemoryLockManager, MemoryLockManager>();

            return services;
        }

        public static IServiceCollection AddMemoryLock(this IServiceCollection services)
        {
            return services.AddMemoryLock(options => { });
        }
    }
}
