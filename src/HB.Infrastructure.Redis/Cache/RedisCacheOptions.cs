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

        public IList<RedisInstanceSetting> ConnectionSettings { get; set; } = new List<RedisInstanceSetting>();

        public string? ApplicationName { get; set; }
    }
}
