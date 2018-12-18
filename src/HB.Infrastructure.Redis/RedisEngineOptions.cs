using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Redis
{
    public class RedisConnectionSetting
    {
        public string InstanceName { get; set; }
        public string ConnectionString { get; set; }
        public bool IsMaster { get; set; }

        //TODO:更改appsettings
    }

    public class RedisEngineOptions : IOptions<RedisEngineOptions>
    {
        public RedisEngineOptions Value { get { return this; } }

        public IList<RedisConnectionSetting> ConnectionSettings { get; set; }

        public RedisEngineOptions()
        {
            ConnectionSettings = new List<RedisConnectionSetting>();
        }

        public RedisConnectionSetting GetConnectionSetting(string instanceName)
        {
            return ConnectionSettings.FirstOrDefault(s => s.InstanceName.Equals(instanceName, GlobalSettings.Comparison));
        }
    }
}
