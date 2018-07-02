using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus.Abstractions
{
    public class EventHandlerConfig
    {
        public string MessageQueueServerName { get; set; }

        public string EventName { get; set; }

        public string SubscribeGroup { get; set; }
    }
}
