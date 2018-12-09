using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.RabbitMQ
{
    public class RabbitMQConnectionSetting
    {
        public string BrokerName { get; set; }

        public string ConnectionString { get; set; }

        public string ExchangeName { get; set; }
    }

    public class RabbitMQEngineOptions : IOptions<RabbitMQEngineOptions>
    {
        public RabbitMQEngineOptions Value => this;

        public IList<RabbitMQConnectionSetting> ConnectionSettings { get; set; } = new List<RabbitMQConnectionSetting>();

        public int NetworkRecoveryIntervalSeconds { get; set; } = 10;

        public int MaxPublishWorkerThread { get; set; } = 300;

        public int MinPublishWorkerThread { get; set; } = 5;

        public RabbitMQConnectionSetting GetConnectionSetting(string brokerName)
        {
            return ConnectionSettings.FirstOrDefault(s => s.BrokerName.Equals(brokerName, GlobalSettings.Comparison));
        }
    }
}
