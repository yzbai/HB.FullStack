using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.DistributedQueue;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.RabbitMQ
{
    public class EventHandlerTaskNode
    {
        public string EventHandlerId { get; set; }

        public Task Task { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }
    }

    public class RabbitMQEventBusEngine : IEventBusEngine
    {
        private ILogger _logger;
        private RabbitMQEngineOptions _options;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _queue;

        private ILogger _consumeTaskManagerLogger;

        //brokerName : PublishTaskManager
        private IDictionary<string, PublishTaskManager> _publishManagers = new Dictionary<string, PublishTaskManager>();

        //brokerName : HistoryTaskManager
        private IDictionary<string, HistoryTaskManager> _historyManager = new Dictionary<string, HistoryTaskManager>();

        //eventType : ConsumeTaskManager
        private IDictionary<string, ConsumeTaskManager> _consumeManager = new Dictionary<string, ConsumeTaskManager>();
        
        public RabbitMQEventBusEngine(IOptions<RabbitMQEngineOptions> options, ILoggerFactory loggerFactory, IRabbitMQConnectionManager connectionManager, IDistributedQueue queue)
        {
            _logger = loggerFactory.CreateLogger<RabbitMQEventBusEngine>();
            _options = options.Value;
            _connectionManager = connectionManager;
            _queue = queue;

            //publish
            ILogger publishTaskManagerLogger = loggerFactory.CreateLogger<PublishTaskManager>();

            foreach (RabbitMQConnectionSetting connectionSetting in _options.ConnectionSettings)
            {
                _publishManagers.Add(connectionSetting.BrokerName, new PublishTaskManager(connectionSetting, _connectionManager, _queue, publishTaskManagerLogger));
            }

            //publish history
            ILogger historyTaskManagerLogger = loggerFactory.CreateLogger<HistoryTaskManager>();

            foreach(RabbitMQConnectionSetting connectionSetting in _options.ConnectionSettings)
            {
                _historyManager.Add(connectionSetting.BrokerName, new HistoryTaskManager(connectionSetting, _connectionManager, _queue, historyTaskManagerLogger));
            }

            //Consume 
            _consumeTaskManagerLogger = loggerFactory.CreateLogger<ConsumeTaskManager>();

        }

        #region Publish

        public async Task<bool> PublishAsync(string brokerName, EventMessage eventMessage)
        {
            //大量Request线程放入缓存池中，离开
            //缓存池内容不能丢，所以用抗击打的Redis来存储
            //注意取消息后需要从kvstore删除

            if (!IsBrokerExists(brokerName))
            {
                return false;
            }

            EventMessageEntity eventEntity = new EventMessageEntity(eventMessage.Type, eventMessage.Body);

            IDistributedQueueResult queueResult = await _queue.Push(queueName: brokerName, data: eventEntity);

            if (queueResult.IsSucceeded())
            {
                NotifyPublishToRabbitMQ(brokerName);

                return true;
            }

            return false;
        }

        private void NotifyPublishToRabbitMQ(string brokerName)
        {
            //让 broker 名字为brokerName的 publishmanager开始工作
            //publishmanager开始创建Task, publishmanager

            //这里已经确保brokerName是存在的了,之前再PublishAsync里已经检查过

            _publishManagers[brokerName].NotifyInComming();
            _historyManager[brokerName].NotifyInComming();
        }

        #endregion

        #region Subscribe

        public bool SubscribeHandler(string brokerName, string eventType, IEventHandler eventHandler)
        {
            if (!IsBrokerExists(brokerName))
            {
                throw new ArgumentException($"当前没有broker为{brokerName}的RabbitMQ。");
            }

            if (_consumeManager.ContainsKey(eventType))
            {
                _logger.LogCritical($"已经存在相同类型的EventHandler了，eventType : {eventType}");

                return false;
            }

            RabbitMQConnectionSetting connectionSetting = _options.GetConnectionSetting(brokerName);

            ConsumeTaskManager manager = new ConsumeTaskManager(eventType, connectionSetting, _connectionManager, _consumeTaskManagerLogger);

            _consumeManager.Add(eventType, manager);

            return true;
        }

        public bool UnSubscribeHandler(string eventyType, string handlerId)
        {
            if (!_consumeManager.ContainsKey(eventyType))
            {
                _logger.LogCritical($"没有这个类型的EventHandler， eventType:{eventyType}");
                return false;
            }

            ConsumeTaskManager manager = _consumeManager[eventyType];

            manager.Cancel();

            _consumeManager.Remove(eventyType);

            return true;
        }

        #endregion

        private bool IsBrokerExists(string brokerName)
        {
            RabbitMQConnectionSetting connectionSetting = _options.GetConnectionSetting(brokerName);

            if (connectionSetting == null)
            {
                _logger.LogCritical($"当前没有broker为{brokerName}的RabbitMQ。");

                return false;
            }

            return true;
        }
    }
}
