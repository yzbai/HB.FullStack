using System;
using System.Collections.Concurrent;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using HB.Framework.KVStore;
using HB.Framework.KVStore.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.RabbitMQ
{
    internal class EventMessageWrapper : KVStoreEntity
    {
        public string BrokerName { get; set; }

        public EventMessage Message { get; set; }

        public EventMessageWrapper() { }

        public EventMessageWrapper(string brokerName, EventMessage message) : this()
        {
            BrokerName = brokerName;
            Message = message;
        }
    }

    public class RabbitMQEventBusEngine : IEventBusEngine
    {
        private ILogger _logger;
        private RabbitMQEngineOptions _options;
        private IRabbitMQConnectionManager _connectionManager;

        private IKVStore _kvStore;


        public RabbitMQEventBusEngine(IOptions<RabbitMQEngineOptions> options, ILogger<RabbitMQEventBusEngine> logger, IRabbitMQConnectionManager connectionManager, IKVStore kvStore)
        {
            _logger = logger;
            _options = options.Value;
            _connectionManager = connectionManager;

            _kvStore = kvStore;

            //初始化 自拥有线程池

            //线程池开始工作
        }

        public void Publish(string brokerName, EventMessage eventMessage)
        {
            //大量Request线程放入缓存池中，离开
            //缓存池内容不能丢，所以用抗击打的Redis来存储

            EventMessageWrapper wrapper = new EventMessageWrapper(brokerName, eventMessage);

            _kvStore.AddAsync(wrapper);
        }
    }
}
