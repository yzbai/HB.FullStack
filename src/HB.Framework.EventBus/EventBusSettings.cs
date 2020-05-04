using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace HB.Framework.EventBus
{
    public class EventSchema
    {
        [DisallowNull, NotNull]
        public string? EventType { get; set; }

        [DisallowNull, NotNull]
        public string? BrokerName { get; set; }

    }

    public class EventBusSettings
    {
        public IList<EventSchema> EventSchemas { get; private set; } = new List<EventSchema>();
    }
}