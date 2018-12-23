using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Infrastructure.Redis.Direct;
using Microsoft.Extensions.Logging;

namespace HB.Infrastructure.RabbitMQ
{
    public abstract class RabbitMQAndDistributedQueueDynamicTaskManager
    {
        protected ILogger _logger;
        protected RabbitMQConnectionSetting _connectionSetting;
        protected IRabbitMQConnectionManager _connectionManager;
        protected IRedisDatabase _redis;

        private ulong _inCommingCount = 0;
        private readonly object _taskNodesLocker = new object();
        private LinkedList<TaskNode> _taskNodes = new LinkedList<TaskNode>();

        protected RabbitMQAndDistributedQueueDynamicTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IRedisDatabase redis, ILogger logger)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _redis = redis;
            _connectionSetting = connectionSetting;

            int taskCount = EstimatedTaskNumber;

            AddTaskAndStart(taskCount > MaxWorkerThread() ? MaxWorkerThread() : taskCount);
        }

        public void NotifyInComming()
        {
            //管理
            //控制Task的数量

            _inCommingCount++;

            if (_inCommingCount > PerThreadFacingCount() || _taskNodes.Count == 0)
            {
                CoordinateTask();
                _inCommingCount = 0;
            }
        }

        protected abstract void TaskProcedure();

        protected abstract ulong PerThreadFacingCount();

        protected abstract string CurrentWorkloadQueueName();

        protected abstract int MaxWorkerThread();

        protected void OnTaskFinished()
        {
            _logger.LogTrace($"Task End, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            CoordinateTask();
        }

        private void AddTaskAndStart(int count)
        {
            _logger.LogTrace($"将要开启{count}个线程, 线程总数量:{_taskNodes.Count}");

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

        private int EstimatedTaskNumber
        {
            get
            {
                //根据初始队列长度，确定线程数量
                ulong length = _redis.QueueLength(redisInstanceName: _connectionSetting.RedisInstanceName, queueName: CurrentWorkloadQueueName());

                if (length == 0)
                {
                    return 0;
                }

                int taskNumber = (int)(length / PerThreadFacingCount()) + 1;

                return taskNumber;
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
                if (EstimatedTaskNumber > _taskNodes.Count)
                {
                    int toAddNumber = EstimatedTaskNumber - _taskNodes.Count;

                    if (toAddNumber + _taskNodes.Count <= MaxWorkerThread())
                    {
                        AddTaskAndStart(toAddNumber);
                    }
                }
            }
        }

        #region DistributedQueue

        protected string DistributedQueueName
        {
            get { return _connectionSetting.BrokerName; }
        }

        protected string DistributedHistoryQueueName
        {
            get { return _connectionSetting.BrokerName + "_History"; }
        }

        protected string DistributedConfirmIdHashName
        {
            get { return _connectionSetting.BrokerName + "_ConfirmSet"; }
        }
    
        #endregion
    }
}
