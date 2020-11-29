using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Lock.Distributed;
using HB.Infrastructure.Redis.DistributedLock;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisDistributedLockServiceRegister
    {
        public static IServiceCollection AddSingleRedisDistributedLock(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<SingleRedisDistributedLockOptions>(configuration);

            services.AddSingleton<IDistributedLockManager, SingleRedisDistributedLockManager>();

            return services;
        }

        public static IServiceCollection AddSingleRedisDistributedLock(this IServiceCollection services, Action<SingleRedisDistributedLockOptions> action)
        {
            services.AddOptions();

            services.Configure(action);

            services.AddSingleton<IDistributedLockManager, SingleRedisDistributedLockManager>();

            return services;
        }
    }
}
