using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using HB.Infrastructure.Redis.Direct;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.RabbitMQ
{
    public class RabbitMQEventBusEngine : IEventBusEngine
    {
        private readonly ILogger _logger;
        private readonly RabbitMQEngineOptions _options;
        private readonly IRabbitMQConnectionManager _connectionManager;
        private readonly IRedisDatabase _redis;

        private readonly ILogger _consumeTaskManagerLogger;

        //brokerName : PublishTaskManager
        private readonly IDictionary<string, PublishTaskManager> _publishManagers = new Dictionary<string, PublishTaskManager>();

        //brokerName : HistoryTaskManager
        private readonly IDictionary<string, HistoryTaskManager> _historyManager = new Dictionary<string, HistoryTaskManager>();

        //eventType : ConsumeTaskManager
        private readonly IDictionary<string, ConsumeTaskManager> _consumeManager = new Dictionary<string, ConsumeTaskManager>();
        
        public RabbitMQEventBusEngine(IOptions<RabbitMQEngineOptions> options, ILoggerFactory loggerFactory, IRabbitMQConnectionManager connectionManager, IRedisDatabase redis)
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
                throw new Exception($"Not exist rabbit broker:{brokerName}");
            }

            EventMessageEntity eventEntity = new EventMessageEntity(eventMessage.Type, eventMessage.JsonData);
            RabbitMQConnectionSetting connectionSetting = _options.GetConnectionSetting(brokerName);

            //推送到一个以broker命名的队列中
            await _redis.PushAsync(redisInstanceName: connectionSetting.RedisInstanceName, queueName: brokerName, data: eventEntity);

            //NotifyPublishToRabbitMQ(brokerName);

            return true;

        }
        //TODO: 考虑让PublishManager和HistoryManager独立
        //TODO： 考虑用redis替代rabbitmq

        //private void NotifyPublishToRabbitMQ(string brokerName)
        //{
        //    //让 broker 名字为brokerName的 publishmanager开始工作
        //    //publishmanager开始创建Task, publishmanager

        //    //这里已经确保brokerName是存在的了,之前再PublishAsync里已经检查过

        //    _publishManagers[brokerName].NotifyInComming();
        //    _historyManager[brokerName].NotifyInComming();
        //}

        #endregion

        #region Subscribe

        public void SubscribeHandler(string brokerName, IEventHandler eventHandler)
        {
            if (!IsBrokerExists(brokerName))
            {
                throw new ArgumentException($"当前没有broker为{brokerName}的RabbitMQ。");
            }

            if (_consumeManager.ContainsKey(eventHandler.EventType))
            {
                string message = $"已经存在相同类型的EventHandler了，eventType : {eventHandler.EventType}";
                _logger.LogCritical(message);

                throw new Exception(message);
            }

            RabbitMQConnectionSetting connectionSetting = _options.GetConnectionSetting(brokerName);

            ConsumeTaskManager manager = new ConsumeTaskManager(eventHandler.EventType, eventHandler, connectionSetting, _connectionManager, _redis, _consumeTaskManagerLogger);

            _consumeManager.Add(eventHandler.EventType, manager);
        }

        public void UnSubscribeHandler(string eventyType)
        {
            if (!_consumeManager.ContainsKey(eventyType))
            {
                string message = $"没有这个类型的EventHandler， eventType:{eventyType}";
                _logger.LogCritical(message);
            }

            ConsumeTaskManager manager = _consumeManager[eventyType];

            manager.Cancel();

            _consumeManager.Remove(eventyType);
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

        public void StartHandle(string eventType)
        {
            
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~RabbitMQEventBusEngine()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
