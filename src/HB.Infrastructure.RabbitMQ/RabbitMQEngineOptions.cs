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

        public int MaxPublishWorkerThread { get; set; } = 10;

        public int MaxSecondsWaitForConfirms { get; set; } = 60;

        public ulong PerThreadFacingEventCount { get; set; } = 10;

        public ulong PerThreadFacingHistoryEventCount { get; set; } = 100;

        /// <summary>
        /// IModel.BasicQos参数
        /// </summary>
        public ushort ConsumePerTimeNumber { get; set; } = 100;
        public int ConsumerAutoRecoveryIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// 一条消息存活的事件，超过这个事件，将被丢弃
        /// </summary>
        public long AliveSeconds { get; set; } = 86400;

        public string RedisInstanceName { get; set; }

        //history队列里的event，等待多少秒后，才能被扫描
        public int WaitSecondsToBeAHistory { get; set; } = 5 * 60;
    }

    public class RabbitMQEngineOptions : IOptions<RabbitMQEngineOptions>
    {
        public RabbitMQEngineOptions Value => this;

        public IList<RabbitMQConnectionSetting> ConnectionSettings { get; set; } = new List<RabbitMQConnectionSetting>();

        public int NetworkRecoveryIntervalSeconds { get; set; } = 10;

        

        public RabbitMQConnectionSetting GetConnectionSetting(string brokerName)
        {
            return ConnectionSettings.FirstOrDefault(s => s.BrokerName.Equals(brokerName, GlobalSettings.Comparison));
        }
    }
}
