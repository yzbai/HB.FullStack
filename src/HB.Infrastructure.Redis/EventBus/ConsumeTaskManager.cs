using HB.Framework.EventBus.Abstractions;
using HB.Infrastructure.Redis.DuplicateCheck;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.EventBus
{
    /// <summary>
    /// TODO: 未来使用多线程, 对于_consumeTask 和 _historyTask
    /// </summary>
    internal class ConsumeTaskManager : IDisposable
    {
        private const int _cONSUME_INTERVAL_SECONDS = 5;
        private const string _hISTORY_REDIS_SCRIPT = "local rawEvent = redis.call('rpop', KEYS[1]) if (not rawEvent) then return 0 end local event = cjson.decode(rawEvent) local aliveTime = ARGV [1] - event[\"Timestamp\"] local eid = event[\"Guid\"] if (aliveTime < ARGV [2] + 0) then redis.call('rpush', KEYS [1], rawEvent) return 1 end if (redis.call('zrank', KEYS [2], eid) ~= nil) then return 2 end redis.call('rpush', KEYS [3], rawEvent) return 3";

        private readonly string _eventType;
        private readonly ILogger _logger;

        private readonly RedisEventBusOptions _options;

        private readonly RedisInstanceSetting _instanceSetting;
        private readonly IEventHandler _eventHandler;

        private readonly Task _consumeTask;
        private readonly CancellationTokenSource _consumeTaskCTS;

        private readonly Task _historyTask;
        private readonly CancellationTokenSource _historyTaskCTS;

        private readonly RedisSetDuplicateChecker _duplicateChecker;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="redisInstanceSetting"></param>
        /// <param name="eventType"></param>
        /// <param name="eventHandler"></param>
        /// <param name="consumeTaskManagerLogger"></param>
        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        public ConsumeTaskManager(
            RedisEventBusOptions options,
            RedisInstanceSetting redisInstanceSetting,
            string eventType,
            IEventHandler eventHandler,
            ILogger consumeTaskManagerLogger)
        {
            _options = options;
            _instanceSetting = redisInstanceSetting;
            _eventType = eventType;
            _eventHandler = eventHandler;
            _logger = consumeTaskManagerLogger;

            _consumeTaskCTS = new CancellationTokenSource();
            _consumeTask = new Task(CosumeTaskProcedure, _consumeTaskCTS.Token, TaskCreationOptions.LongRunning);

            _historyTaskCTS = new CancellationTokenSource();
            _historyTask = new Task(HistoryTaskProcedure, _historyTaskCTS.Token, TaskCreationOptions.LongRunning);

            _duplicateChecker = new RedisSetDuplicateChecker(_instanceSetting, _options.EventBusEventMessageExpiredHours * 60 * 60, _logger);
        }

        private void HistoryTaskProcedure()
        {
            while (!_historyTaskCTS.IsCancellationRequested)
            {
                try
                {
                    /*
                    -- keys = {history_queue, acks_sortedset, queue}
                    -- argvs={currentTimestampSeconds, waitSecondsToBeHistory}

                    local rawEvent = redis.call('rpop', KEYS[1])
                    
                    --还没有数据
                    if (not rawEvent)
                    then
                        return 0
                    end

                    local event = cjson.decode(rawEvent)
                    local aliveTime = ARGV [1] - event["Timestamp"]
                    local eid = event["Guid"]

                    --如果太新，就直接放回去，然后返回
                    if (aliveTime < ARGV [2] + 0)
                    then
                        redis.call('rpush', KEYS [1], rawEvent)
                        return 1
                    end

                    --如果已存在acks set中，则直接返回
                    if (redis.call('zrank', KEYS [2], eid) ~= nil)
                    then
                        return 2
                    end

                    --说明还没有被处理，遗忘了，放回处理队列
                    redis.call('rpush', KEYS [3], rawEvent)
                    return 3
                    */

                    string[] redisKeys = new string[] {
                        RedisEventBusEngine.HistoryQueueName(_eventType),
                        RedisEventBusEngine.AcksSetName(_eventType),
                        RedisEventBusEngine.QueueName(_eventType)
                    };

                    string[] redisArgvs = new string[] {
                        TimeUtil.CurrentTimestampSeconds().ToString(GlobalSettings.Culture),
                        _options.EventBusConsumerAckTimeoutSeconds.ToString(GlobalSettings.Culture)
                    };

                    IDatabase database = RedisInstanceManager.GetDatabaseAsync(_instanceSetting, _logger).Result;

                    //TODO: Use LoadedScript
                    int result = (int)database.ScriptEvaluate(
                        _hISTORY_REDIS_SCRIPT,
                        redisKeys.Select<string, RedisKey>(t => t).ToArray(),
                        redisArgvs.Select<string, RedisValue>(t => t).ToArray());

                    if (result == 0)
                    {
                        //还没有数据，等会吧
                        _logger.LogTrace($"ScanHistory {_instanceSetting.InstanceName} 中,还没有数据，，EventType:{_eventType}");
                        Thread.Sleep(10 * 1000);
                    }
                    else if (result == 1)
                    {
                        //时间太早，等会再检查
                        _logger.LogTrace($"ScanHistory {_instanceSetting.InstanceName} 中,数据还太新，一会再检查，，EventType:{_eventType}");
                        Thread.Sleep(10 * 1000);
                    }
                    else if (result == 2)
                    {
                        //成功
                        _logger.LogTrace($"ScanHistory {_instanceSetting.InstanceName} 中,消息已被处理，现在移出History，EventType:{_eventType}");
                    }
                    else if (result == 3)
                    {
                        //重新放入队列再发送
                        _logger.LogWarning($"ScanHistory {_instanceSetting.InstanceName} 中,消息可能被遗漏， 重新放入队列，，EventType:{_eventType}");
                    }
                    else
                    {
                        //出错
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"ScanHistory {_instanceSetting.InstanceName} 中出错，EventType:{_eventType}, Exceptions: {ex.Message}");
                    //throw;
                }
            }
        }

        /// <summary>
        /// CosumeTaskProcedure
        /// </summary>
        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        /// <exception cref="AggregateException">Ignore.</exception>
        private void CosumeTaskProcedure()
        {
            while (!_consumeTaskCTS.IsCancellationRequested)
            {
                //1, Get Entity
                IDatabase database = RedisInstanceManager.GetDatabaseAsync(_instanceSetting, _logger).Result;

                RedisValue redisValue = database.ListRightPopLeftPush(RedisEventBusEngine.QueueName(_eventType), RedisEventBusEngine.HistoryQueueName(_eventType));

                if (redisValue.IsNullOrEmpty)
                {
                    _logger.LogTrace($"ConsumeTask Sleep, brokerName:{_instanceSetting.InstanceName}, eventType:{_eventType}");

                    Thread.Sleep(_cONSUME_INTERVAL_SECONDS * 1000);

                    continue;
                }

                EventMessageEntity entity = SerializeUtil.FromJson<EventMessageEntity>(redisValue)!;

                //2, 过期检查

                double spendHours = (TimeUtil.CurrentTimestampSeconds() - entity.Timestamp) / 3600;

                if (spendHours > _options.EventBusEventMessageExpiredHours)
                {
                    _logger.LogCritical($"有EventMessage过期，eventType:{_eventType}, entity:{SerializeUtil.ToJson(entity)}");
                    continue;
                }

                //3, 防重检查

                string AcksSetName = RedisEventBusEngine.AcksSetName(_eventType);

                if (!_duplicateChecker.Lock(AcksSetName, entity.Guid, out string token))
                {
                    //竟然有人在检查entity.Guid,好了，这下肯定有人在处理了，任务结束。哪怕那个人没处理成功，也没事，等着history吧。
                    continue;
                }

                bool? isExist = _duplicateChecker.IsExistAsync(AcksSetName, entity.Guid, token).Result;

                if (isExist == null || isExist.Value)
                {
                    _logger.LogInformation($"有EventMessage重复，eventType:{_eventType}, entity:{SerializeUtil.ToJson(entity)}");

                    _duplicateChecker.Release(AcksSetName, entity.Guid, token);

                    continue;
                }

                //4, Handle Entity
                try
                {
                    _eventHandler.Handle(entity.JsonData);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    _logger.LogCritical(ex, $"处理消息出错, eventType:{_eventType}, entity : {SerializeUtil.ToJson(entity)}");
                }

                //5, Acks
                _duplicateChecker.AddAsync(AcksSetName, entity.Guid, entity.Timestamp, token).Wait();
                _duplicateChecker.Release(AcksSetName, entity.Guid, token);
            }
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        /// <exception cref="AggregateException">Ignore.</exception>
        public void Cancel()
        {
            _consumeTaskCTS.Cancel();
            _historyTaskCTS.Cancel();
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        /// <exception cref="TaskSchedulerException">Ignore.</exception>
        public void Start()
        {
            _consumeTask.Start(TaskScheduler.Default);
            _historyTask.Start(TaskScheduler.Default);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        /// <exception cref="AggregateException">Ignore.</exception>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _consumeTaskCTS?.Cancel();

                    while (!_consumeTask.IsCompleted)
                    {
                        Thread.Sleep(1 * 1000);
                    }

                    _consumeTask.Dispose();
                    _consumeTaskCTS?.Dispose();

                    _historyTaskCTS?.Cancel();

                    while (!_historyTask.IsCompleted)
                    {
                        Thread.Sleep(1 * 1000);
                    }

                    _historyTask.Dispose();
                    _historyTaskCTS?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
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

