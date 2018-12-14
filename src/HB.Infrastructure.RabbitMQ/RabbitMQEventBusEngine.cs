using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.DistributedQueue;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace HB.Infrastructure.RabbitMQ
{
    public class RabbitMQEventBusEngine : IEventBusEngine
    {
        private ILogger _logger;
        private RabbitMQEngineOptions _options;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _queue;
        private IDictionary<string, PublishTaskManager> _publishManagers;
        private IDictionary<string, HistoryTaskManager> _historyManager;
        
        public RabbitMQEventBusEngine(IOptions<RabbitMQEngineOptions> options, ILoggerFactory loggerFactory, IRabbitMQConnectionManager connectionManager, IDistributedQueue queue)
        {
            _logger = loggerFactory.CreateLogger<RabbitMQEventBusEngine>();
            _options = options.Value;
            _connectionManager = connectionManager;

            _queue = queue;

            _publishManagers = new Dictionary<string, PublishTaskManager>();
            _historyManager = new Dictionary<string, HistoryTaskManager>();


            
            //publish
            ILogger publishTaskManagerLogger = loggerFactory.CreateLogger<PublishTaskManager>();

            foreach (RabbitMQConnectionSetting connectionSetting in _options.ConnectionSettings)
            {
                _publishManagers.Add(connectionSetting.BrokerName, new PublishTaskManager(connectionSetting, _connectionManager, _queue, publishTaskManagerLogger));
            }

            //history
            ILogger historyTaskManagerLogger = loggerFactory.CreateLogger<HistoryTaskManager>();

            foreach(RabbitMQConnectionSetting connectionSetting in _options.ConnectionSettings)
            {
                _historyManager.Add(connectionSetting.BrokerName, new HistoryTaskManager(connectionSetting, _connectionManager, _queue, historyTaskManagerLogger));
            }
        }

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
