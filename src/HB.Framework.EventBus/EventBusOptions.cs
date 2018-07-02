using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus
{
    public class EventConfig
    {
        public string EventName { get; set; }

        public string MessageQueueServerName { get; set; }
    }

    public class PublishConfig
    {
        public int RetryCount { get; set; } = 3;

        public IList<EventConfig> Events;
    }

    public class SubscribeConfig
    {
        public string DefaultSubscribeGroup { get; set; }
    }

    public class EventBusOptions : IOptions<EventBusOptions>
    {
        public EventBusOptions Value => this;

        public PublishConfig PublishConfig { get; set; }

        public SubscribeConfig SubscribeConfig { get; set; }
    }
}