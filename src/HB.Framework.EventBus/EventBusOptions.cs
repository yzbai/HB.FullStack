using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus
{
    public class EventConfig
    {
        public string EventName { get; set; }

        public string ServerName { get; set; }
    }

    public class PublishConfig
    {
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// 有哪些事件，这些事件需要提前登记在appsetting中，或者动态添加事件配置
        /// </summary>
        public IList<EventConfig> Events;
    }

    public class SubscribeConfig
    {
        /// <summary>
        /// 订阅者属于的默认组
        /// </summary>
        public string DefaultSubscribeGroup { get; set; }
    }

    public class EventBusOptions : IOptions<EventBusOptions>
    {
        public EventBusOptions Value => this;

        public PublishConfig PublishConfig { get; set; }

        public SubscribeConfig SubscribeConfig { get; set; }
    }
}