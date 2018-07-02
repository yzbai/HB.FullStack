using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Redis
{
    public class RedisConnectionSetting
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public bool IsMaster { get; set; }
    }

    public class RedisEngineOptions : IOptions<RedisEngineOptions>
    {
        public RedisEngineOptions Value { get { return this; } }

        public IList<RedisConnectionSetting> ConnectionSettings { get; set; }

        public RedisEngineOptions()
        {
            ConnectionSettings = new List<RedisConnectionSetting>();
        }
    }
}
