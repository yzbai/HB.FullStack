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
        //TODO:更改appsettings

        /// <summary>
        /// 消费者得到一条消息后，必须在ConsumerAckTimeoutSeconds之内，发出ack，否则认为没有被合理处理，将重发。
        /// 即在多少秒之后，history中的消息被认为是history，可以处理了。从进入history队列开始算。
        /// </summary>
        public int EventBusConsumerAckTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 一条消息过期时间
        /// </summary>
        public int EventBusEventMessageExpiredHours { get; set; } = 24;
    }

    public class RedisOptions : IOptions<RedisOptions>
    {
        public RedisOptions Value { get { return this; } }

        public IList<RedisInstanceSetting> ConnectionSettings { get; } = new List<RedisInstanceSetting>();

        public RedisInstanceSetting GetInstanceSetting(string instanceName)
        {
            return ConnectionSettings.FirstOrDefault(s => s.InstanceName.Equals(instanceName, GlobalSettings.Comparison));
        }
    }
}
