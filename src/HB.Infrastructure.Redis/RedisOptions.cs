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

        public int EventBusMaxThreads { get; set; } = 10;

        /// <summary>
        /// 每多多少Message就增加一个线程
        /// </summary>
        public int EventBusThreadAcceleration { get; set; } = 50;

        /// <summary>
        /// 消费者得到一条消息后，必须在ConsumerAckTimeoutSeconds之内，发出ack，否则认为没有被合理处理，将重发。
        /// </summary>
        public int EventBusConsumerAckTimeoutSeconds { get; set; } = 30;
    }

    public class RedisOptions : IOptions<RedisOptions>
    {
        public RedisOptions Value { get { return this; } }

        public IList<RedisConnectionSetting> ConnectionSettings { get; set; }

        public RedisOptions()
        {
            ConnectionSettings = new List<RedisConnectionSetting>();
        }

        public RedisConnectionSetting GetConnectionSetting(string instanceName)
        {
            return ConnectionSettings.FirstOrDefault(s => s.InstanceName.Equals(instanceName, GlobalSettings.Comparison));
        }
    }
}
