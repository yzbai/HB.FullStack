using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Redis.DistributedLock
{
    public class SingleRedisDistributedLockOptions : IOptions<SingleRedisDistributedLockOptions>
    {
        public SingleRedisDistributedLockOptions Value => this;

        public string? ApplicationName { get; set; }

        public RedisInstanceSetting ConnectionSetting { get; set; } = null!;

        public int DefaultWaitMilliseconds { get; set; } = 60 * 1000;

        public int DefaultRetryIntervalMilliseconds { get; set; } = 500;

    }
}
