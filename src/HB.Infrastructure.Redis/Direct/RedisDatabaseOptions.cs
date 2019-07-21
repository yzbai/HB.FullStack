using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.Direct
{
    public class RedisDatabaseOptions : IOptions<RedisDatabaseOptions>
    {
        public RedisDatabaseOptions Value {
            get {
                return this;
            }
        }

        public IList<RedisInstanceSetting> ConnectionSettings { get; } = new List<RedisInstanceSetting>();

    }
}
