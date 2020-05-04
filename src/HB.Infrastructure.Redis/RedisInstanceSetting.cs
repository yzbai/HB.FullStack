using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace HB.Infrastructure.Redis
{
    public class RedisInstanceSetting
    {
        [DisallowNull, NotNull]
        public string? InstanceName { get; set; }

        [DisallowNull, NotNull]
        public string? ConnectionString { get; set; }

        public int DatabaseNumber { get; set; } = 0;
    }
}
