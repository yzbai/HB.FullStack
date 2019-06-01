using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.Common.Utility;
using HB.Framework.EventBus.Abstractions;
using HB.Infrastructure.Redis.DuplicateCheck;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.EventBus
{
    /// <summary>
    /// TODO: 未来使用多线程, 对于_consumeTask 和 _historyTask
    /// </summary>
    internal class ConsumeTaskManager : IDisposable
    {
        private const int CONSUME_INTERVAL_SECONDS = 5;
        private const string HISTORY_REDIS_SCRIPT = "local rawEvent = redis.call('rpop', KEYS[1]) if (not rawEvent) then return 0 end local event = cjson.decode(rawEvent) local aliveTime = ARGV [1] - event[\"Timestamp\"] local eid = event[\"Guid\"] if (aliveTime < ARGV [2] + 0) then redis.call('rpush', KEYS [1], rawEvent) return 1 end if (redis.call('zrank', KEYS [2], eid) ~= nil) then return 2 end redis.call('rpush', KEYS [3], rawEvent) return 3";
        private readonly string _instanceName;
        private readonly string _eventType;
        private readonly ILogger _logger;
        private readonly IRedisInstanceManager _instanceManager;
        private readonly RedisInstanceSetting _instanceSetting;
        private readonly IEventHandler _eventHandler;

        private readonly Task _consumeTask;
        private readonly CancellationTokenSource _consumeTaskCTS;

        private readonly Task _historyTask;
        private readonly CancellationTokenSource _historyTaskCTS;

        private readonly DuplicateChecker _duplicateChecker;

        public ConsumeTaskManager(
            string brokerName, 
            IRedisInstanceManager instanceManager, 
            string eventType, 
            IEventHandler eventHandler, 
            ILogger consumeTaskManagerLogger)
        {
            _instanceName = brokerName;
            _instanceManager = instanceManager;
            _instanceSetting = _instanceManager.GetInstanceSetting(brokerName);
            _eventType = eventType;
            _eventHandler = eventHandler;
            _logger = consumeTaskManagerLogger;

            _consumeTaskCTS = new CancellationTokenSource();
            _consumeTask = new Task(CosumeTaskProcedure, _consumeTaskCTS.Token, TaskCreationOptions.LongRunning);

            _historyTaskCTS = new CancellationTokenSource();
            _historyTask = new Task(HistoryTaskProcedure, _historyTaskCTS.Token, TaskCreationOptions.LongRunning);

            _duplicateChecker = new DuplicateChecker(_instanceManager, _instanceName, _instanceSetting.EventBusEventMessageExpiredHours * 60 * 60);
        }

        private void HistoryTaskProcedure()
        {
            while(!_historyTaskCTS.IsCancellationRequested)
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
                        _instanceSetting.EventBusConsumerAckTimeoutSeconds.ToString(GlobalSettings.Culture)
                    };

                    IDatabase database = _instanceManager.GetDatabase(_instanceName);

                    //TODO: Use LoadedScript
                    int result = (int)database.ScriptEvaluate(
                        HISTORY_REDIS_SCRIPT,
                        redisKeys.Select<string, RedisKey>(t => t).ToArray(),
                        redisArgvs.Select<string, RedisValue>(t => t).ToArray());

                    if (result == 0)
                    {
                        //还没有数据，等会吧
                        _logger.LogTrace($"ScanHistory {_instanceName} 中,还没有数据，，EventType:{_eventType}");
                        Thread.Sleep(10 * 1000);
                    }
                    else if (result == 1)
                    {
                        //时间太早，等会再检查
                        _logger.LogTrace($"ScanHistory {_instanceName} 中,数据还太新，一会再检查，，EventType:{_eventType}");
                        Thread.Sleep(10 * 1000);
                    }
                    else if (result == 2)
                    {
                        //成功
                        _logger.LogTrace($"ScanHistory {_instanceName} 中,消息已被处理，现在移出History，EventType:{_eventType}");
                    }
                    else if (result == 3)
                    {
                        //重新放入队列再发送
                        _logger.LogWarning($"ScanHistory {_instanceName} 中,消息可能被遗漏， 重新放入队列，，EventType:{_eventType}");
                    }
                    else
                    {
                        //出错
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogCritical(ex, $"ScanHistory {_instanceName} 中，EventType:{_eventType}, Exceptions: {ex.Message}");
                    throw;
                }
            }
        }

        private void CosumeTaskProcedure()
        {
            while (!_consumeTaskCTS.IsCancellationRequested)
            {
                //1, Get Entity
                IDatabase database = _instanceManager.GetDatabase(_instanceName);

                RedisValue redisValue = database.ListRightPopLeftPush(RedisEventBusEngine.QueueName(_eventType), RedisEventBusEngine.HistoryQueueName(_eventType));

                if (redisValue.IsNullOrEmpty)
                {
                    _logger.LogTrace($"ConsumeTask Sleep, brokerName:{_instanceName}, eventType:{_eventType}");

                    Thread.Sleep(CONSUME_INTERVAL_SECONDS * 1000);

                    continue;
                }

                EventMessageEntity entity = JsonUtil.FromJson<EventMessageEntity>(redisValue);

                //2, 过期检查

                double spendHours = (TimeUtil.CurrentTimestampSeconds() - entity.Timestamp) / 3600;

                if (spendHours > _instanceSetting.EventBusEventMessageExpiredHours)
                {
                    _logger.LogCritical($"有EventMessage过期，eventType:{_eventType}, entity:{JsonUtil.ToJson(entity)}");
                    continue;
                }

                //3, 防重检查

                string AcksSetName = RedisEventBusEngine.AcksSetName(_eventType);
                string token = string.Empty;

                if (!_duplicateChecker.Lock(AcksSetName, entity.Guid, out token))
                {
                    //竟然有人在检查entity.Guid,好了，这下肯定有人在处理了，任务结束。哪怕那个人没处理成功，也没事，等着history吧。
                    continue;  
                }

                bool? isExist = _duplicateChecker.IsExist(AcksSetName, entity.Guid, token);

                if (isExist == null || isExist.Value)
                {
                    _logger.LogInformation($"有EventMessage重复，eventType:{_eventType}, entity:{JsonUtil.ToJson(entity)}");

                    _duplicateChecker.Release(AcksSetName, entity.Guid, token);

                    continue;
                }

                //4, Handle Entity
                try
                {
                    _eventHandler.Handle(entity.JsonData);
                }
                catch(Exception ex)
                {
                    _logger.LogCritical(ex, $"处理消息出错, eventType:{_eventType}, entity : {JsonUtil.ToJson(entity)}");
                    throw;
                }

                //5, Acks
                _duplicateChecker.Add(AcksSetName, entity.Guid, entity.Timestamp, token);
                _duplicateChecker.Release(AcksSetName, entity.Guid, token);
            }
        }

        public void Cancel()
        {
            _consumeTaskCTS.Cancel();
            _historyTaskCTS.Cancel();
        }

        public void Start()
        {
            _consumeTask.Start(TaskScheduler.Default);
            _historyTask.Start(TaskScheduler.Default);
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

                    while (!_consumeTask.IsCompleted)
                    {
                        Thread.Sleep(1 * 1000);
                    }

                    _consumeTask.Dispose();
                    _consumeTaskCTS?.Dispose();

                    _historyTaskCTS?.Cancel();

                    while(!_historyTask.IsCompleted)
                    {
                        Thread.Sleep(1 * 1000);
                    }

                    _historyTask.Dispose();
                    _historyTaskCTS?.Dispose();
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

