using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.EventBus;
using HB.FullStack.EventBus.Abstractions;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.EventBus
{
    /// <summary>
    /// brokerName 就是 InstanceName
    /// </summary>
    internal class RedisEventBusEngine : IEventBusEngine
    {
        private readonly ILogger _logger;

        private readonly object _consumeTaskManagerLocker = new object();

        private readonly object _consumTaskCloseLocker = new object();

        private readonly RedisEventBusOptions _options;

        private readonly IDistributedLockManager _lockManager;

        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;

        //eventType : ConsumeTaskManager
        private readonly IDictionary<string, ConsumeTaskManager> _consumeTaskManagers = new Dictionary<string, ConsumeTaskManager>();

        public RedisEventBusEngine(IOptions<RedisEventBusOptions> options, ILogger<RedisEventBusEngine> logger, IDistributedLockManager lockManager)
        {
            _logger = logger;
            _options = options.Value;
            _lockManager = lockManager;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);

            _logger.LogInformation($"RedisEventBusEngine初始化完成");
        }

        /// <summary>
        /// PublishAsync
        /// </summary>
        /// <param name="brokerName"></param>
        /// <param name="eventMessage"></param>
        /// <returns></returns>

        public async Task PublishAsync(string brokerName, string eventName, string jsonData)
        {
            RedisInstanceSetting instanceSetting = GetRedisInstanceSetting(brokerName);

            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(instanceSetting, _logger).ConfigureAwait(false);

            EventMessageEntity entity = new EventMessageEntity(eventName, jsonData);

            await database.ListLeftPushAsync(QueueName(entity.EventName), SerializeUtil.ToJson(entity)).ConfigureAwait(false);

        }

        //启动Consume线程, 启动History线程
        /// <summary>
        /// StartHandle
        /// </summary>
        /// <param name="eventType"></param>

        public void StartHandle(string eventType)
        {
            if (!_consumeTaskManagers.ContainsKey(eventType))
            {
                throw new EventBusException($"Handler Not Existed for EventType:{eventType}");
            }

            _consumeTaskManagers[eventType].Start();
        }

        /// <summary>
        /// 每一种事件，只有一次SubscribeHandler的机会。之后再订阅，就报错了。
        /// 开始处理
        /// </summary>

        public void SubscribeHandler(string brokerName, string eventType, IEventHandler eventHandler)
        {
            RedisInstanceSetting instanceSetting = GetRedisInstanceSetting(brokerName);

            lock (_consumeTaskManagerLocker)
            {
                if (_consumeTaskManagers.ContainsKey(eventType))
                {
                    throw new EventBusException($"Handler already exists for EventType: {eventType}, BrokerName:{brokerName}");
                }

                ConsumeTaskManager consumeTaskManager = new ConsumeTaskManager(_options, instanceSetting, _lockManager, eventType, eventHandler, _logger);

                _consumeTaskManagers.Add(eventType, consumeTaskManager);
            }
        }

        /// <summary>
        /// 停止处理
        /// </summary>

        public async Task UnSubscribeHandlerAsync(string eventType)
        {
            await _consumeTaskManagers[eventType].CancelAsync().ConfigureAwait(false);

            _consumeTaskManagers[eventType].Dispose();

            lock (_consumeTaskManagerLocker)
            {
                if (!_consumeTaskManagers.ContainsKey(eventType))
                {
                    throw new EventBusException($"Handler for EventType:{eventType} not Exist.");
                }

                //_consumeTaskManagers[eventType] = null;
                _consumeTaskManagers.Remove(eventType);
            }
        }

        public static string QueueName(string eventType)
        {
            return eventType;
        }

        public static string HistoryQueueName(string eventType)
        {
            return eventType + "_History";
        }

        public static string AcksSetName(string eventType)
        {
            return eventType + "_Acks";
        }

        public EventBusSettings EventBusSettings
        {
            get
            {
                return _options.EventBusSettings;
            }
        }

        public void Close()
        {
            lock (_consumTaskCloseLocker)
            {
                foreach (var kv in _consumeTaskManagers)
                {
                    kv.Value.Dispose();
                };
            }
        }


        /// <summary>
        /// GetRedisInstanceSetting
        /// </summary>
        /// <param name="brokerName"></param>
        /// <returns></returns>

        private RedisInstanceSetting GetRedisInstanceSetting(string brokerName)
        {
            if (!_instanceSettingDict.TryGetValue(brokerName, out RedisInstanceSetting instanceSetting))
            {
                throw new EventBusException($"Not Found matched RedisInstanceSetting for Broker: {brokerName}.");
            }

            return instanceSetting;
        }
    }
}
