using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.DistributedQueue;
using Microsoft.Extensions.Logging;

namespace HB.Infrastructure.RabbitMQ
{
    public class HistoryTaskManager
    {
        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _distributedQueue;

        private ulong _inCommingCount = 0;
        private object _taskNodesLocker = new object();
        private LinkedList<TaskNode> _taskNodes = new LinkedList<TaskNode>();

        public HistoryTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IDistributedQueue distributedQueue, ILogger logger)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _distributedQueue = distributedQueue;
            _connectionSetting = connectionSetting;

            AddTask(InitialTaskNumber);
        }

        private void TaskProcedure()
        {
            _logger.LogTrace($"ScanHistory Task Start, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            try
            {
                while (true)
                {
                    IDistributedQueueResult result = _distributedQueue.PopHistoryToQueueIfNotExistInHash<EventMessageEntity>(historyQueue: DistributedHistoryQueueName, queue: DistributedQueueName, hash: DistributedConfirmIdHashName);

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

        private ulong PerThreadFacingCount
        {
            get { return _connectionSetting.PerThreadFacingHistoryEventCount; }
        }

        public void NotifyInComming()
        {
            //管理
            //控制Task的数量

            _inCommingCount++;

            if (_inCommingCount > PerThreadFacingCount)
            {
                CoordinateTask();
                _inCommingCount = 0;
            }
        }

        private int InitialTaskNumber
        {
            get
            {
                //根据初始队列长度，确定线程数量
                ulong length = _distributedQueue.Length(queueName: DistributedHistoryQueueName);

                if (length == 0)
                {
                    return 0;
                }

                int taskNumber = (int)(length / PerThreadFacingCount) + 1;

                return taskNumber;
            }
        }

        private void AddTask(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                lock (_taskNodesLocker)
                {
                    CancellationTokenSource cs = new CancellationTokenSource();

                    Task task = Task.Factory.StartNew(TaskProcedure, cs.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    TaskNode taskNode = new TaskNode() { Task = task, CancellationTokenSource = cs };

                    _taskNodes.AddLast(taskNode);
                }
            }
        }

        private void CoordinateTask()
        {
            lock (_taskNodesLocker)
            {
                //first clean finished tasks
                LinkedListNode<TaskNode> node = _taskNodes.First;

                while (node != null)
                {
                    LinkedListNode<TaskNode> nextNode = node.Next;

                    if (node.Value.Task.IsCompleted)
                    {
                        _taskNodes.Remove(node);
                    }

                    node = nextNode;
                }

                //caculate task number
                if (InitialTaskNumber > _taskNodes.Count)
                {
                    int toAddNumber = InitialTaskNumber - _taskNodes.Count;

                    AddTask(toAddNumber);
                }
            }
        }

        private void OnTaskFinished()
        {
            _logger.LogTrace($"HistoryToRabbitMQ Task End, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            CoordinateTask();
        }

        #region DistributedQueue

        private string DistributedQueueName
        {
            get { return _connectionSetting.BrokerName; }
        }

        private string DistributedHistoryQueueName
        {
            get { return _connectionSetting.BrokerName + "_History"; }
        }

        private string DistributedConfirmIdHashName
        {
            get { return _connectionSetting.BrokerName + "_ConfirmSet"; }
        }

        #endregion
    }
}
