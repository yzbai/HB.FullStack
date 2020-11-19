using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.Cache
{
    public class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        public RedisCacheOptions Value => this;

        public string? DefaultInstanceName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:集合属性应为只读", Justification = "<挂起>")]
        public IList<RedisInstanceSetting> ConnectionSettings { get; set; } = new List<RedisInstanceSetting>();
    }
}
