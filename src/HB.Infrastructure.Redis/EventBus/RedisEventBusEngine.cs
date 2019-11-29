using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
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

        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;

        private readonly IDictionary<string, ConsumeTaskManager> _consumeTaskManagers = new Dictionary<string, ConsumeTaskManager>();//eventType : ConsumeTaskManager

        public RedisEventBusEngine(IOptions<RedisEventBusOptions> options, ILogger<RedisEventBusEngine> logger)
        {
            _logger = logger;
            _options = options.Value;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);
        }

        public async Task<bool> PublishAsync(string brokerName, EventMessage eventMessage)
        {
            RedisInstanceSetting instanceSetting = GetRedisInstanceSetting(brokerName);

            if (instanceSetting == null)
            {
                return false;
            }

            IDatabase database = RedisInstanceManager.GetDatabase(instanceSetting, _logger);

            EventMessageEntity entity = new EventMessageEntity(eventMessage.Type, eventMessage.JsonData);

            await database.ListLeftPushAsync(QueueName(entity.Type), SerializeUtil.ToJson(entity)).ConfigureAwait(false);

            return true;
        }

        //启动Consume线程, 启动History线程
        public void StartHandle(string eventType)
        {
            if (!_consumeTaskManagers.ContainsKey(eventType))
            {
                throw new ArgumentException($"不存在{eventType}的处理程序。");
            }

            _consumeTaskManagers[eventType].Start();
        }

        /// <summary>
        /// 每一种事件，只有一次SubscribeHandler的机会。之后再订阅，就报错了。
        /// 开始处理
        /// </summary>
        public bool SubscribeHandler(string brokerName, string eventType, IEventHandler eventHandler)
        {
            RedisInstanceSetting instanceSetting = GetRedisInstanceSetting(brokerName);

            if (instanceSetting == null)
            {
                return false;
            }

            lock (_consumeTaskManagerLocker)
            {
                if (_consumeTaskManagers.ContainsKey(eventType))
                {
                    throw new ArgumentException($"已经存在{eventType}的处理程序.");
                }

                ConsumeTaskManager consumeTaskManager = new ConsumeTaskManager(_options, instanceSetting, eventType, eventHandler, _logger);

                _consumeTaskManagers.Add(eventType, consumeTaskManager);

                return true;
            }
        }
        /// <summary>
        /// 停止处理
        /// </summary>
        public void UnSubscribeHandler(string eventType)
        {
            lock (_consumeTaskManagerLocker)
            {
                if (!_consumeTaskManagers.ContainsKey(eventType))
                {
                    throw new ArgumentException($"不存在{eventType}的处理程序。");
                }

                _consumeTaskManagers[eventType].Cancel();

                _consumeTaskManagers[eventType].Dispose();

                _consumeTaskManagers[eventType] = null;
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

        public EventBusSettings EventBusSettings {
            get {
                return _options.EventBusSettings;
            }
        }

        public void Close()
        {
            lock(_consumTaskCloseLocker)
            {
                _consumeTaskManagers.ForEach(kv => {
                    kv.Value.Dispose();
                });
            }
        }

        private RedisInstanceSetting GetRedisInstanceSetting(string brokerName)
        {
            if (!_instanceSettingDict.TryGetValue(brokerName, out RedisInstanceSetting instanceSetting))
            {
                _logger.LogCritical($"no matched broker {brokerName} found.");

                return null;
            }

            return instanceSetting;
        }
    }
}
