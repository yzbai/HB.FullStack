using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.EventBus.Abstractions;
using HB.Infrastructure.Redis.DuplicateCheck;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.EventBus
{
    /// <summary>
    /// TODO: 未来使用多线程
    /// </summary>
    public class ConsumeTaskManager : IDisposable
    {
        private const int CONSUME_INTERVAL_SECONDS = 5;

        private string _brokerName;
        private string _eventType;
        private ILogger _logger;
        private IRedisInstanceManager _connectionManager;
        private RedisInstanceSetting _redisInstanceSetting;
        private IEventHandler _eventHandler;

        private Task _consumeTask;
        private CancellationTokenSource _consumeTaskCTS;

        private Task _historyTask;
        private CancellationTokenSource _historyTaskCTS;

        private IDuplicateChecker _duplicateChecker;

        public ConsumeTaskManager(string brokerName, IRedisInstanceManager connectionManager, string eventType, IEventHandler eventHandler, IDuplicateChecker duplicateChecker, ILogger consumeTaskManagerLogger)
        {
            _brokerName = brokerName;
            _connectionManager = connectionManager;
            _redisInstanceSetting = _connectionManager.GetInstanceSetting(brokerName, true);
            _eventType = eventType;
            _eventHandler = eventHandler;
            _logger = consumeTaskManagerLogger;

            _consumeTaskCTS = new CancellationTokenSource();
            _consumeTask = new Task(CosumeTaskProcedure, _consumeTaskCTS.Token, TaskCreationOptions.LongRunning);

            _historyTaskCTS = new CancellationTokenSource();
            _historyTask = new Task(HistoryTaskProcedure, _historyTaskCTS.Token, TaskCreationOptions.LongRunning);

            _duplicateChecker = duplicateChecker;
        }

        private void HistoryTaskProcedure()
        {
            throw new NotImplementedException();
        }

        private void CosumeTaskProcedure()
        {
            while (true)
            {
                //1, Get Entity
                IDatabase database = GetDatabase(_brokerName);

                RedisValue redisValue = database.ListRightPopLeftPush(RedisEventBusEngine.QueueName(_eventType), RedisEventBusEngine.HistoryQueueName(_eventType));

                if (redisValue.IsNullOrEmpty)
                {
                    _logger.LogTrace($"ConsumeTask Sleep, brokerName:{_brokerName}, eventType:{_eventType}");

                    Thread.Sleep(CONSUME_INTERVAL_SECONDS * 1000);

                    continue;
                }

                EventMessageEntity entity = DataConverter.To<EventMessageEntity>(redisValue);

                //2, 过期检查

                double spendHours = (DataConverter.CurrentTimestampSeconds() - entity.Timestamp) / 3600;

                if (spendHours > _redisInstanceSetting.EventBusEventMessageExpiredHours)
                {
                    _logger.LogCritical($"有EventMessage过期，eventType:{_eventType}, entity:{DataConverter.ToJson(entity)}");
                    continue;
                }

                //3, 防重检查

                //4, Handle Entity
                try
                {
                    _eventHandler.Handle(entity.JsonData);
                }
                catch(Exception ex)
                {
                    _logger.LogCritical(ex, $"处理消息出错, eventType:{_eventType}, entity : {DataConverter.ToJson(entity)}");
                }

                

                //5, Acks
                database.SetAdd()
            }
        }

        public void Cancel()
        {
            _consumeTaskCTS.Cancel();
        }

        public void Start()
        {
            _consumeTask.Start(TaskScheduler.Default);
        }

        private IDatabase GetDatabase(string brokerName)
        {
            return _connectionManager.GetDatabase(brokerName, 0, true);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _consumeTaskCTS?.Cancel();
                    _consumeTaskCTS.Dispose();
                    _consumeTask.Dispose();

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~ConsumeTaskManager()
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
