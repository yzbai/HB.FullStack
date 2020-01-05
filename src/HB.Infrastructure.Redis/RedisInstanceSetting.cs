using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Redis
{
    public class RedisInstanceSetting
    {
        public string InstanceName { get; set; }
        public string ConnectionString { get; set; }

        public int DatabaseNumber { get; set; } = 0;
    }
}
