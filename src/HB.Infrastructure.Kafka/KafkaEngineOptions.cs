using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.Infrastructure.Kafka
{

    public class KafkaServer
    {
        public string Name { get; set; }

        public string Host { get; set; }

        public IDictionary<string, string> ProducerConfig;

        public IDictionary<string, string> ConsumerConfig;

    }

    public class KafkaEngineOptions : IOptions<KafkaEngineOptions>
    {
        public KafkaEngineOptions Value => this;

        public int ProducerFlushWaitSeconds { get; set; } = 10;

        public int ConsumerCancellWaitSeconds { get; set; } = 5;

        public int ConsumerPollSeconds { get; set; } = 1;

        public IList<KafkaServer> Servers { get; set; }

        public KafkaServer GetServerInfo(string serverName)
        {
            if (Servers == null)
            {
                return null;
            }

            return Servers.First(s => s.Name.Equals(serverName));
        }
        
    }
}
