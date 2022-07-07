using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.EventBus;
using HB.FullStack.EventBus.Abstractions;
using HB.FullStack.Lock.Distributed;
using HB.Infrastructure.Redis.Shared;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.EventBus
{
    /// <summary>
    /// brokerName 就是 InstanceName
    /// </summary>
    public class RedisEventBusEngine : IEventBusEngine
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

            _logger.LogInformation("RedisEventBusEngine初始化完成");
        }

        public async Task PublishAsync(string brokerName, string eventName, string jsonData)
        {
            RedisInstanceSetting instanceSetting = GetRedisInstanceSetting(brokerName);

            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(instanceSetting, _logger).ConfigureAwait(false);

            EventMessageModel model = new EventMessageModel(eventName, jsonData);

            await database.ListLeftPushAsync(QueueName(model.EventName), SerializeUtil.ToJson(model)).ConfigureAwait(false);
        }

        /// <summary>
        /// StartHandle启动Consume线程, 启动History线程
        /// </summary>
        public void StartHandle(string eventName)
        {
            if (!_consumeTaskManagers.ContainsKey(eventName))
            {
                throw Exceptions.NoHandler(eventType: eventName);
            }

            _consumeTaskManagers[eventName].Start();
        }

        /// <summary>
        /// 每一种事件，只有一次SubscribeHandler的机会。之后再订阅，就报错了。
        /// 开始处理
        /// </summary>
        public void SubscribeHandler(string brokerName, string eventName, IEventHandler eventHandler)
        {
            RedisInstanceSetting instanceSetting = GetRedisInstanceSetting(brokerName);

            lock (_consumeTaskManagerLocker)
            {
                if (_consumeTaskManagers.ContainsKey(eventName))
                {
                    throw Exceptions.HandlerAlreadyExisted(eventType: eventName, brokerName: brokerName);
                }

                ConsumeTaskManager consumeTaskManager = new ConsumeTaskManager(_options, instanceSetting, _lockManager, eventName, eventHandler, _logger);

                _consumeTaskManagers.Add(eventName, consumeTaskManager);
            }
        }

        /// <summary>
        /// 停止处理
        /// </summary>
        public async Task UnSubscribeHandlerAsync(string eventyName)
        {
            await _consumeTaskManagers[eventyName].CancelAsync().ConfigureAwait(false);

            _consumeTaskManagers[eventyName].Dispose();

            lock (_consumeTaskManagerLocker)
            {
                if (!_consumeTaskManagers.ContainsKey(eventyName))
                {
                    throw Exceptions.NoHandler(eventyName);
                }

                //_consumeTaskManagers[eventType] = null;
                _consumeTaskManagers.Remove(eventyName);
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

        private RedisInstanceSetting GetRedisInstanceSetting(string brokerName)
        {
            if (!_instanceSettingDict.TryGetValue(brokerName, out RedisInstanceSetting? instanceSetting))
            {
                throw Exceptions.SettingsError(brokerName, $"Not Found matched RedisInstanceSetting");
            }

            return instanceSetting;
        }
    }
}