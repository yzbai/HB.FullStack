using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using HB.Infrastructure.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.RabbitMQ
{
    public class RabbitMQEventBusEngine : IEventBusEngine
    {
        private ILogger _logger;
        private RabbitMQEngineOptions _options;
        private readonly IRabbitMQConnectionManager _connectionManager;
        private IRedisEngine _redis;

        private readonly ILogger _consumeTaskManagerLogger;

        //brokerName : PublishTaskManager
        private IDictionary<string, PublishTaskManager> _publishManagers = new Dictionary<string, PublishTaskManager>();

        //brokerName : HistoryTaskManager
        private IDictionary<string, HistoryTaskManager> _historyManager = new Dictionary<string, HistoryTaskManager>();

        //eventType : ConsumeTaskManager
        private IDictionary<string, ConsumeTaskManager> _consumeManager = new Dictionary<string, ConsumeTaskManager>();
        
        public RabbitMQEventBusEngine(IOptions<RabbitMQEngineOptions> options, ILoggerFactory loggerFactory, IRabbitMQConnectionManager connectionManager, IRedisEngine redis)
        {
            _logger = loggerFactory.CreateLogger<RabbitMQEventBusEngine>();
            _options = options.Value;
            _connectionManager = connectionManager;
            _redis = redis;

            //publish
            ILogger publishTaskManagerLogger = loggerFactory.CreateLogger<PublishTaskManager>();

            foreach (RabbitMQConnectionSetting connectionSetting in _options.ConnectionSettings)
            {
                _publishManagers.Add(connectionSetting.BrokerName, new PublishTaskManager(connectionSetting, _connectionManager, _redis, publishTaskManagerLogger));
            }

            //publish history
            ILogger historyTaskManagerLogger = loggerFactory.CreateLogger<HistoryTaskManager>();

            foreach(RabbitMQConnectionSetting connectionSetting in _options.ConnectionSettings)
            {
                _historyManager.Add(connectionSetting.BrokerName, new HistoryTaskManager(connectionSetting, _connectionManager, _redis, historyTaskManagerLogger));
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

            EventMessageEntity eventEntity = new EventMessageEntity(eventMessage.Type, eventMessage.JsonData);
            RabbitMQConnectionSetting connectionSetting = _options.GetConnectionSetting(brokerName);

            await _redis.PushAsync(redisInstanceName: connectionSetting.RedisInstanceName, queueName: brokerName, data: eventEntity);

            NotifyPublishToRabbitMQ(brokerName);

            return true;
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

            ConsumeTaskManager manager = new ConsumeTaskManager(eventType, eventHandler, connectionSetting, _connectionManager, _redis, _consumeTaskManagerLogger);

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
