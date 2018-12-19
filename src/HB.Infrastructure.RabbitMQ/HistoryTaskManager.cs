using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Infrastructure.Redis;
using Microsoft.Extensions.Logging;

namespace HB.Infrastructure.RabbitMQ
{
    public class HistoryTaskManager : RabbitMQAndDistributedQueueDynamicTaskManager
    {
        public HistoryTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IRedisEngine redis, ILogger logger)
            : base(connectionSetting, connectionManager, redis, logger) { }

        protected override void TaskProcedure()
        {
            _logger.LogTrace($"ScanHistory Task Start, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            try
            {
                while (true)
                {
                    RedisEngineResult result = _redis.PopAndPushIfNotExist<EventMessageEntity>(
                        redisInstanceName: _connectionSetting.RedisInstanceName,
                        historyQueue: DistributedHistoryQueueName, 
                        queue: DistributedQueueName, 
                        hashName: DistributedConfirmIdHashName);

                    if (result.HistoryDeleted)
                    {

                    }
                    else if (result.HistoryReback)
                    {

                    }
                    else if (result.HistoryShouldWait)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"ScanHistory {_connectionSetting.BrokerName} 中，Thread Id : {Thread.CurrentThread.ManagedThreadId}, Exceptions: {ex.Message}");
            }
            finally
            {
                //Thread.Sleep();
                OnTaskFinished();
            }
        }

        protected override ulong PerThreadFacingCount()
        {
            return _connectionSetting.PerThreadFacingHistoryEventCount;
        }

        protected override string CurrentWorkloadQueueName()
        {
            return DistributedHistoryQueueName;
        }
    }
}
