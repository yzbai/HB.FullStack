using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HB.Framework.EventBus
{
    public class TopicSchema
    {
        public string Name { get; set; }

        public string BrokerName { get; set; }
    }

    public class EventBusOptions : IOptions<EventBusOptions>
    {
        public EventBusOptions Value => this;

        public IList<TopicSchema> TopicShemas { get; set; } = new List<TopicSchema>();

        public TopicSchema GetTopicSchema(string topic)
        {
            return TopicShemas.FirstOrDefault(t => t.Name.Equals(topic, GlobalSettings.Comparison));
        }
    }
}