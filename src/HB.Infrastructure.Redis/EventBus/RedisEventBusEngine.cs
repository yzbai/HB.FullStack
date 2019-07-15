using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.EventBus
{
    internal class RedisEventBusEngine : IEventBusEngine
    {
        private readonly IRedisInstanceManager _instanceManager;
        private readonly ILogger _consumeTaskManagerLogger;

        private readonly object _consumeTaskManagerLocker;
        //eventType : ConsumeTaskManager
        private readonly IDictionary<string, ConsumeTaskManager> _consumeTaskManagers;

        public RedisEventBusEngine(IRedisInstanceManager connectionManager, ILoggerFactory loggerFactory)
        {
            _instanceManager = connectionManager;
            _consumeTaskManagerLogger = loggerFactory.CreateLogger<ConsumeTaskManager>();
            _consumeTaskManagers = new Dictionary<string, ConsumeTaskManager>();
            _consumeTaskManagerLocker = new object();
        }

        public async Task<bool> PublishAsync(string brokerName, EventMessage eventMessage)
        {
            IDatabase database = _instanceManager.GetDatabase(brokerName);

            if (database == null)
            {
                return false;
            }

            EventMessageEntity entity = new EventMessageEntity(eventMessage.Type, eventMessage.JsonData);

            await database.ListLeftPushAsync(QueueName(entity.Type), JsonUtil.ToJson(entity)).ConfigureAwait(false);

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
        public void SubscribeHandler(string brokerName, string eventType, IEventHandler eventHandler)
        {
            IDatabase database = _instanceManager.GetDatabase(brokerName);

            if (database == null)
            {
                throw new ArgumentException($"不存在Redis实例{brokerName}");
            }

            lock (_consumeTaskManagerLocker)
            {
                if (_consumeTaskManagers.ContainsKey(eventType))
                {
                    throw new ArgumentException($"已经存在{eventType}的处理程序.");
                }

                ConsumeTaskManager consumeTaskManager = new ConsumeTaskManager(brokerName,  _instanceManager, eventType, eventHandler, _consumeTaskManagerLogger);

                _consumeTaskManagers.Add(eventType, consumeTaskManager);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var kv in _consumeTaskManagers)
                    {
                        kv.Value.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~RedisEventBusEngine()
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
