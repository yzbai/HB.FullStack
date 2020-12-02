using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.EventBus
{
    public class EventSchema
    {
        [DisallowNull, NotNull]
        public string? EventName { get; set; }

        /// <summary>
        /// 比如RedisEventBus中的ConnectionSettings的InstanceName。即处理这个event的设施
        /// </summary>
        [DisallowNull, NotNull]
        public string? BrokerName { get; set; }

    }

    public class EventBusSettings
    {
        public IList<EventSchema> EventSchemas { get; private set; } = new List<EventSchema>();
    }
}