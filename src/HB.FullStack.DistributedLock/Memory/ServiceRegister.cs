using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HB.FullStack.Lock;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryLockManagerServiceRegister
    {
        public static IServiceCollection AddMemoryLock(this IServiceCollection services)
        {
            return services.AddMemoryLock(options => { });
        }

        public static IServiceCollection AddMemoryLock(this IServiceCollection services, IConfiguration configuration)
        {
            AddMemoryCacheIfNone(services);

            services.Configure<MemoryLockOptions>(configuration);

            services.AddSingleton<IMemoryLockManager, MemoryLockManager>();

            return services;
        }

        public static IServiceCollection AddMemoryLock(this IServiceCollection services, Action<MemoryLockOptions> action)
        {
            AddMemoryCacheIfNone(services);

            services.Configure(action);

            services.AddSingleton<IMemoryLockManager, MemoryLockManager>();

            return services;
        }

        private static void AddMemoryCacheIfNone(IServiceCollection services)
        {
            if (!services.Any(sd => sd.ServiceType == typeof(IMemoryCache)))
            {
                services.AddMemoryCache();
                //throw LockExceptions.MemoryLockError(cause: "MemoryLockManager需要MemoryCache服务");
            }
        }

    }
}
