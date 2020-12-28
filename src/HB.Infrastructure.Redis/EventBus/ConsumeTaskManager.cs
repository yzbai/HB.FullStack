using AsyncAwaitBestPractices;

using HB.FullStack.EventBus.Abstractions;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.EventBus
{
    internal class ConsumeTaskManager : IDisposable
    {
        private const int _cONSUME_INTERVAL_SECONDS = 5;


        /// <summary>
        ///  -- keys = {history_queue, acks_sortedset, queue}
        ///  -- argvs={currentTimestampSeconds, waitSecondsToBeHistory
        /// </summary>
        private const string _hISTORY_REDIS_SCRIPT = @"
local rawEvent = redis.call('rpop', KEYS[1]) 

--还没有数据
if (not rawEvent) then 
    return 0 
end 
local event = cjson.decode(rawEvent) 
local aliveTime = ARGV[1] - event['Timestamp'] 
local eid = event['Guid'] 

 --如果太新，就直接放回去，然后返回
if (aliveTime < ARGV[2] + 0) then 
    redis.call('rpush', KEYS[1], rawEvent) 
    return 1 
end 

--如果已存在acks set中，则直接返回
if (redis.call('zrank', KEYS[2], eid) ~= nil) then 
    -- 移除acks队列    
    redis.call('zrem', KEYS[2], eid) 
    return 2 
end

--说明还没有被处理，遗忘了，放回处理队列
redis.call('rpush', KEYS[3], rawEvent) return 3";

        private readonly string _eventType;
        private readonly ILogger _logger;

        private readonly RedisEventBusOptions _options;

        private readonly RedisInstanceSetting _instanceSetting;
        private readonly IDistributedLockManager _lockManager;
        private readonly IEventHandler _eventHandler;

        private Task? _consumeTask;
        private Task? _historyTask;
        private readonly CancellationTokenSource _consumeTaskCTS;
        private readonly CancellationTokenSource _historyTaskCTS;

        private readonly long _eventBusEventMessageExpiredSeconds;

        private byte[] _loadedHistoryLua = null!;

        public ConsumeTaskManager(
            RedisEventBusOptions options,
            RedisInstanceSetting redisInstanceSetting,
            IDistributedLockManager lockManager,
            string eventType,
            IEventHandler eventHandler,
            ILogger logger)
        {
            _options = options;
            _instanceSetting = redisInstanceSetting;
            _lockManager = lockManager;

            _eventBusEventMessageExpiredSeconds = _options.EventBusEventMessageExpiredHours * 60 * 60;

            _eventType = eventType;
            _eventHandler = eventHandler;
            _logger = logger;

            _consumeTaskCTS = new CancellationTokenSource();

            _historyTaskCTS = new CancellationTokenSource();

            InitLodedLua();
        }

        private void InitLodedLua()
        {
            IServer server = RedisInstanceManager.GetServer(_instanceSetting, _logger);

            _loadedHistoryLua = server.ScriptLoad(_hISTORY_REDIS_SCRIPT);
        }

        private async Task ScanHistoryAsync(CancellationToken cancellationToken)
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    RedisKey[] redisKeys = new RedisKey[] {
                        RedisEventBusEngine.HistoryQueueName(_eventType),
                        RedisEventBusEngine.AcksSetName(_eventType),
                        RedisEventBusEngine.QueueName(_eventType)
                        };

                    RedisValue[] redisArgvs = new RedisValue[] {
                        TimeUtil.UtcNowUnixTimeSeconds,
                        _options.EventBusConsumerAckTimeoutSeconds
                        };

                    IDatabase database = await RedisInstanceManager.GetDatabaseAsync(_instanceSetting, _logger).ConfigureAwait(false);

                    int result = (int)await database.ScriptEvaluateAsync(
                        _loadedHistoryLua,
                        redisKeys,
                        redisArgvs).ConfigureAwait(false);

                    if (result == 0)
                    {
                        //还没有数据，等会吧
                        _logger.LogTrace($"ScanHistory {_instanceSetting.InstanceName} 中,还没有数据，，EventType:{_eventType}");
                        await Task.Delay(10 * 1000, cancellationToken).ConfigureAwait(false);
                    }
                    else if (result == 1)
                    {
                        //时间太早，等会再检查
                        _logger.LogTrace($"ScanHistory {_instanceSetting.InstanceName} 中,数据还太新，一会再检查，，EventType:{_eventType}");
                        await Task.Delay(10 * 1000, cancellationToken).ConfigureAwait(false);
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
                catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
                {
                    _logger.LogError(ex, "NOSCRIPT, will try again.");

                    InitLodedLua();

                    continue;
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogError(ex, $"Scan History 中出现Redis链接问题. EventType:{_eventType}");
                    await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
                }
                catch (RedisTimeoutException ex)
                {
                    _logger.LogError(ex, $"Scan History 中出现Redis超时问题. EventType:{_eventType}");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Scan History 出现未知问题. EventType:{_eventType}");
                }
            }

            _logger.LogTrace($"History Task For {_eventType} Stopped.");
        }

        /// <summary>
        /// CosumeTaskProcedure
        /// </summary>


        private async Task CosumeAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    long now = TimeUtil.UtcNowUnixTimeSeconds;

                    //1, Get Entity
                    IDatabase database = await RedisInstanceManager.GetDatabaseAsync(_instanceSetting, _logger).ConfigureAwait(false);

                    RedisValue redisValue = await database.ListRightPopLeftPushAsync(RedisEventBusEngine.QueueName(_eventType), RedisEventBusEngine.HistoryQueueName(_eventType)).ConfigureAwait(false);

                    if (redisValue.IsNullOrEmpty)
                    {
                        _logger.LogTrace($"ConsumeTask Sleep, brokerName:{_instanceSetting.InstanceName}, eventType:{_eventType}");

                        await Task.Delay(_cONSUME_INTERVAL_SECONDS * 1000, cancellationToken).ConfigureAwait(false);

                        continue;
                    }

                    EventMessageEntity? entity = SerializeUtil.FromJson<EventMessageEntity>(redisValue);

                    if (entity == null)
                    {
                        _logger.LogCritical($"有空EventMessageEntity, eventType:{_eventType}, value:{redisValue}");
                        continue;
                    }

                    using IDistributedLock distributedLock = await _lockManager.NoWaitLockAsync(
                        "eBusC_" + entity.Guid,
                        TimeSpan.FromSeconds(_options.EventBusConsumerAckTimeoutSeconds),
                        false,
                        cancellationToken).ConfigureAwait(false);

                    if (!distributedLock.IsAcquired)
                    {
                        //竟然有人在检查entity.Guid,好了，这下肯定有人在处理了，任务结束。哪怕那个人没处理成功，也没事，等着history吧。
                        continue;
                    }

                    //2, 过期检查

                    if (now - entity.Timestamp > _eventBusEventMessageExpiredSeconds)
                    {
                        _logger.LogCritical($"有EventMessage过期，eventType:{_eventType}, entity:{SerializeUtil.ToJson(entity)}");

                        await distributedLock.DisposeAsync().ConfigureAwait(false);

                        continue;
                    }

                    //3, 防重检查

                    string AcksSetName = RedisEventBusEngine.AcksSetName(_eventType);

                    bool? isExist = await IsAcksExistedAsync(database, AcksSetName, entity.Guid).ConfigureAwait(false);

                    if (isExist == null || isExist.Value)
                    {
                        _logger.LogInformation($"有EventMessage重复，eventType:{_eventType}, entity:{SerializeUtil.ToJson(entity)}");

                        await distributedLock.DisposeAsync().ConfigureAwait(false);

                        continue;
                    }

                    //4, Handle Entity
                    try
                    {
                        await _eventHandler.HandleAsync(entity.JsonData, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, $"处理消息出错, eventType:{_eventType}, entity : {SerializeUtil.ToJson(entity)}");
                    }

                    //5, Acks
                    await AddAcksAsync(now, database, AcksSetName, entity.Guid, entity.Timestamp).ConfigureAwait(false);

                    _logger.LogTrace($"Consume Task For {_eventType} Stopped.");
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogError(ex, $"Consume 中出现Redis链接问题. EventType:{_eventType}");
                    await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
                }
                catch (RedisTimeoutException ex)
                {
                    _logger.LogError(ex, $"Consume 中出现Redis超时问题. EventType:{_eventType}");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Consume 出现未知问题. EventType:{_eventType}");
                }
            }
        }

        #region Acks

        private static async Task<bool> IsAcksExistedAsync(IDatabase database, string setName, string guid)
        {
            if (await database.SortedSetScoreAsync(setName, guid).ConfigureAwait(false) == null)
            {
                return false;
            }

            return true;
        }

        private async Task AddAcksAsync(long now, IDatabase database, string setName, string entityGuid, long entityTimestamp)
        {
            await database.SortedSetAddAsync(setName, entityGuid, entityTimestamp, CommandFlags.None).ConfigureAwait(false);

            await ClearExpiredAcksAsync(now, database, setName).ConfigureAwait(false);
        }

        private async Task ClearExpiredAcksAsync(long now, IDatabase database, string setName)
        {
            long stopTimestamp = now - _eventBusEventMessageExpiredSeconds;

            //寻找小于stopTimestamp的，删除他们
            await database.SortedSetRemoveRangeByScoreAsync(setName, 0, stopTimestamp).ConfigureAwait(false);

        }

        #endregion

        /// <summary>
        /// Cancel
        /// </summary>


        public async Task CancelAsync()
        {
            _consumeTaskCTS.Cancel();
            _historyTaskCTS.Cancel();

            List<Task> tasks = new List<Task>();

            if (_consumeTask != null)
            {
                tasks.Add(_consumeTask);
            }

            if (_historyTask != null)
            {
                tasks.Add(_historyTask);
            }

            if (tasks.Any())
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            _logger.LogTrace($"Task For {_eventType} Stopped.");
        }

        /// <summary>
        /// Start
        /// </summary>



        public void Start()
        {
            _consumeTask = CosumeAsync(_consumeTaskCTS.Token);

            _consumeTask.Fire();


            _historyTask = ScanHistoryAsync(_historyTaskCTS.Token);

            _historyTask.Fire();
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _consumeTaskCTS?.Cancel();
                    _historyTaskCTS?.Cancel();

                    _consumeTaskCTS?.Dispose();
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

