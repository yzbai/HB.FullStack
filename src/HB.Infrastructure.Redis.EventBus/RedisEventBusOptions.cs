using HB.FullStack.EventBus;
using HB.Infrastructure.Redis.Shared;

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.EventBus
{
    public class RedisEventBusOptions : IOptions<RedisEventBusOptions>
    {
        public RedisEventBusOptions Value => this;

        public IList<RedisInstanceSetting> ConnectionSettings { get; } = new List<RedisInstanceSetting>();


        /// <summary>
        /// 消费者得到一条消息后，必须在ConsumerAckTimeoutSeconds之内，发出ack，否则认为没有被合理处理，将重发。
        /// 即在多少秒之后，history中的消息被认为是history，可以处理了。从进入history队列开始算。
        /// </summary>
        public int EventBusConsumerAckTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 一条消息过期时间
        /// </summary>
        public int EventBusEventMessageExpiredHours { get; set; } = 72;

        public EventBusSettings EventBusSettings { get; set; } = new EventBusSettings();
    }
}
