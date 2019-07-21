using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HB.Framework.EventBus
{
    public class EventSchema
    {
        public string EventType { get; set; }

        public string BrokerName { get; set; }

    }

    public class EventBusSettings
    {
        public IList<EventSchema> EventSchemas { get; } = new List<EventSchema>();
    }
}