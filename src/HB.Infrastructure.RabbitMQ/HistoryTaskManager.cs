using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.DistributedQueue;
using Microsoft.Extensions.Logging;

namespace HB.Infrastructure.RabbitMQ
{
    public class HistoryTaskManager : RabbitMQAndDistributedQueueDynamicTaskManager
    {
        public HistoryTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IDistributedQueue distributedQueue, ILogger logger)
            : base(connectionSetting, connectionManager, distributedQueue, logger) { }

        protected override void TaskProcedure()
        {
            _logger.LogTrace($"ScanHistory Task Start, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            try
            {
                while (true)
                {
                    IDistributedQueueResult result = _distributedQueue.PopHistoryToQueueIfNotExistInHash<EventMessageEntity>(
                        historyQueue: DistributedHistoryQueueName, 
                        queue: DistributedQueueName, 
                        hash: DistributedConfirmIdHashName);

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
