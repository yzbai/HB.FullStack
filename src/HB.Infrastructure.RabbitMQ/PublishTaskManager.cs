using System;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.DistributedQueue;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace HB.Infrastructure.RabbitMQ
{
    /// <summary>
    /// 每一个PublishWokerManager，负责一个broker的工作，自己决定工作量的大小
    /// eventType = routingkey = queuename
    /// </summary>
    public class PublishTaskManager
    {
        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _distributedQueue;
                
        //EventMessageEntity.Type
        private ConcurrentDictionary<string, bool> _eventDeclareDict = new ConcurrentDictionary<string, bool>();

        public PublishTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IDistributedQueue distributedQueue, ILogger logger)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _distributedQueue = distributedQueue;
            _connectionSetting = connectionSetting;

            AddPublishTask(InitialPublishTaskNumber);
            AddHistoryTask(InitialHistoryTaskNumber);
        }

        public void NotifyPublishComming()
        {
            //管理
            //控制Task的数量

            _inCommingPublishCount++;

            if (_inCommingPublishCount > _connectionSetting.PerThreadFacingEventCount)
            {
                CoordinatePublishTask();
                _inCommingPublishCount = 0;
            }

            _inCommingHistoryCount++;

            if (_inCommingHistoryCount > _connectionSetting.PerThreadFacingHistoryCount)
            {
                CoordinateHistoryTask();
                _inCommingHistoryCount = 0;
            }
        }

        private void PublishToRabbitMQ()
        {
            //per thread per channel . await前后线程不同。想让前后线程一致。Task里避免用await，用wait()

            _logger.LogTrace($"PublishToRabbitMQ Task Start, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            IModel channel = null;

            List<string> confirmEventIdList = new List<string>();
            
            ulong fs = 1; //the seqno of first in confirmEventIdList

            object confirmEventIdListLocker = new object();

            try
            {
                channel = CreateChannel(brokerName: _connectionSetting.BrokerName, isPublish: true);

                channel.BasicAcks += (sender, eventArgs) =>
                {
                    List<string> deleteIds = new List<string>();

                    lock (confirmEventIdListLocker)
                    {
                        ulong seqno = eventArgs.DeliveryTag;
                        int index = (int)(seqno - fs);

                        if (eventArgs.Multiple)
                        {
                            for (int i = 0; i < index; i++)
                            {
                                deleteIds.Add(confirmEventIdList[i]);
                                confirmEventIdList[i] = null;
                            }
                        }
                        else
                        {
                            deleteIds.Add(confirmEventIdList[index]);
                            confirmEventIdList[index] = null;
                        }

                        // 收缩,每100次，收缩一次
                        if (seqno % 100 == 0 && confirmEventIdList.Count > 100)
                        {
                            //奇点处理,直接当成nack
                            confirmEventIdList[0] = null;

                            int nextIndex = 0;

                            for (int i = 0; i < confirmEventIdList.Count; ++i)
                            {
                                if (confirmEventIdList[i] != null)
                                {
                                    nextIndex = i;
                                    break;
                                }
                            }

                            fs = fs + (ulong)nextIndex;

                            confirmEventIdList.RemoveRange(0, nextIndex);
                            confirmEventIdList.TrimExcess();
                        }
                    }

                    //将deleteIds放入 DistributedConfirmEventIdSet

                    List<int> valueList = new List<int>();

                    foreach (string id in deleteIds)
                    {
                        valueList.Add(1);
                    }

                    IDistributedQueueResult result = _distributedQueue.AddIntToHash(hashName: DistributedConfirmIdHashName, fields: deleteIds, values: valueList);
                };

                channel.BasicNacks += (sender, eventArgs) =>
                {
                    List<string> deleteIds = new List<string>();

                    //那就在history里待着吧，等待回收
                    lock (confirmEventIdListLocker)
                    {
                        ulong seqno = eventArgs.DeliveryTag;
                        int index = (int)(seqno - fs);

                        if (eventArgs.Multiple)
                        {
                            for (int i = 0; i < index; i++)
                            {
                                deleteIds.Add(confirmEventIdList[i]);
                                confirmEventIdList[i] = null;
                            }
                        }
                        else
                        {
                            deleteIds.Add(confirmEventIdList[index]);
                            confirmEventIdList[index] = null;
                        }
                    }

                    //将deleteIds放入 DistributedConfirmEventIdSet

                    List<int> valueList = new List<int>();

                    foreach (string id in deleteIds)
                    {
                        valueList.Add(0);
                    }

                    IDistributedQueueResult result = _distributedQueue.AddIntToHash(hashName: DistributedConfirmIdHashName, fields: deleteIds, values: valueList);
                };

                while (true)
                {
                    //获取数据.用同步方法，不能用await，避免前后线程不一致
                    IDistributedQueueResult queueResult = _distributedQueue.PopAndPush<EventMessageEntity>(fromQueueName: DistributedQueueName, toQueueName: DistributedQueueHistoryName);

                    //没有数据，queue为空，直接推出
                    if (!queueResult.IsSucceeded() || !EventMessageEntity.IsValid(queueResult.Data))
                    {
                        //可能性1：Queue挂掉，取不到；可能性2：Queue已空
                        //退出循环, 结束Thread

                        bool allAcks = channel.WaitForConfirms(TimeSpan.FromSeconds(_connectionSetting.MaxSecondsWaitForConfirms), out bool timedOut);

                        if (allAcks && !timedOut)
                        {
                            _logger.LogDebug($"Task will end, no in queue : {DistributedQueueName}, and all good.");
                        }
                        else if (allAcks && timedOut)
                        {
                            _logger.LogWarning($"Task will end, WaitForConfirms timedOut, with broker : {_connectionSetting.BrokerName}, and senconds : {_connectionSetting.MaxSecondsWaitForConfirms}");
                        }
                        else
                        {
                            _logger.LogError($"Task will end, and there have nacks, with broker : {_connectionSetting.BrokerName}");
                        }

                        break;
                    }

                    EventMessageEntity eventEntity = queueResult.Data as EventMessageEntity;

                    //Channel 不可用，直接结束Thread
                    if (channel == null || channel.CloseReason != null)
                    {
                        _logger.LogWarning($"Channel for broker: {_connectionSetting.BrokerName}, has closed, reason:{channel?.CloseReason.ToString()}");

                        break;
                    }

                    //Declare Queue & Binding
                    DeclareEventType(channel, eventEntity);
                    
                    //publish
                    IBasicProperties basicProperties = channel.CreateBasicProperties();
                    basicProperties.DeliveryMode = 2;

                    channel.BasicPublish(_connectionSetting.ExchangeName, EventTypeToRoutingKey(eventEntity.Type), true, basicProperties, eventEntity.Body);

                    //Confirm
                    confirmEventIdList.Add(eventEntity.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"在PublishToRabbitMQ {_connectionSetting.BrokerName} 中，Thread Id : {Thread.CurrentThread.ManagedThreadId}, Exceptions: {ex.Message}");
            }
            finally
            {
                channel?.Close();

                OnPublishTaskFinished();
            }
        }

        private void ScanHistory()
        {
            _logger.LogTrace($"ScanHistory Task Start, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            try
            {
                string queueName = DistributedQueueName;
                string historyQueueName = DistributedHistoryQueueName;
                string hashName = DistributedConfirmIdHashName;

                while (true)
                {
                    IDistributedQueueResult result = _distributedQueue.PopHistoryToQueueIfNotExistInHash<EventMessageEntity>(historyQueue: historyQueueName, queue: queueName, hash: hashName);

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
            catch(Exception ex)
            {
                _logger.LogCritical(ex, $"ScanHistory {_connectionSetting.BrokerName} 中，Thread Id : {Thread.CurrentThread.ManagedThreadId}, Exceptions: {ex.Message}");
            }
            finally
            {
                //Thread.Sleep();
                OnHistoryTaskFinished();
            }
        }

        #region Publish Task

        private ulong _inCommingPublishCount = 0;
        private object _publishTaskNodesLocker = new object();
        private LinkedList<TaskNode> _publishTaskNodes = new LinkedList<TaskNode>();

        private int InitialPublishTaskNumber
        {
            get
            {
                //根据初始队列长度，确定线程数量
                ulong length = _distributedQueue.Length(queueName: DistributedQueueName);

                if (length == 0)
                {
                    return 0;
                }

                int taskNumber = (int)(length / _connectionSetting.PerThreadFacingEventCount) + 1;

                return taskNumber;
            }

        }

        private void AddPublishTask(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                lock (_publishTaskNodesLocker)
                {
                    CancellationTokenSource cs = new CancellationTokenSource();

                    Task task = Task.Factory.StartNew(PublishToRabbitMQ, cs.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    TaskNode taskNode = new TaskNode() { Task = task, CancellationTokenSource = cs };

                    _publishTaskNodes.AddLast(taskNode);
                }
            }
        }

        private void CoordinatePublishTask()
        {
            lock(_publishTaskNodesLocker)
            {
                //first clean finished tasks
                LinkedListNode<TaskNode> node = _publishTaskNodes.First;

                while (node != null)
                {
                    LinkedListNode<TaskNode> nextNode = node.Next;

                    if (node.Value.Task.IsCompleted)
                    {
                        _publishTaskNodes.Remove(node);
                    }

                    node = nextNode;
                }

                //caculate task number
                if (InitialPublishTaskNumber > _publishTaskNodes.Count)
                {
                    int toAddNumber = InitialPublishTaskNumber - _publishTaskNodes.Count;

                    AddPublishTask(toAddNumber);
                }
            }
        }

        private void OnPublishTaskFinished()
        {
            _logger.LogTrace($"PublishToRabbitMQ Task End, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            CoordinatePublishTask();
        }

        #endregion

        #region History Task

        private ulong _inCommingHistoryCount = 0;
        private object _historyTaskNodesLocker = new object();
        private LinkedList<TaskNode> _historyTaskNodes = new LinkedList<TaskNode>();

        private int InitialHistoryTaskNumber
        {
            get
            {
                //根据初始队列长度，确定线程数量
                ulong length = _distributedQueue.Length(queueName: DistributedHistoryQueueName);

                if (length == 0)
                {
                    return 0;
                }

                int taskNumber = (int)(length / _connectionSetting.PerThreadFacingHistoryCount) + 1;

                return taskNumber;
            }
        }

        private void AddHistoryTask(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                lock(_historyTaskNodesLocker)
                {
                    CancellationTokenSource cs = new CancellationTokenSource();

                    Task task = Task.Factory.StartNew(ScanHistory, cs.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    TaskNode taskNode = new TaskNode() { Task = task, CancellationTokenSource = cs };

                    _historyTaskNodes.AddLast(taskNode);
                }
            }
        }

        private void CoordinateHistoryTask()
        {
            lock (_historyTaskNodesLocker)
            {
                //first clean finished tasks
                LinkedListNode<TaskNode> node = _historyTaskNodes.First;

                while (node != null)
                {
                    LinkedListNode<TaskNode> nextNode = node.Next;

                    if (node.Value.Task.IsCompleted)
                    {
                        _historyTaskNodes.Remove(node);
                    }

                    node = nextNode;
                }

                //caculate task number
                if (InitialHistoryTaskNumber > _historyTaskNodes.Count)
                {
                    int toAddNumber = InitialHistoryTaskNumber - _historyTaskNodes.Count;

                    AddHistoryTask(toAddNumber);
                }
            }
        }

        private void OnHistoryTaskFinished()
        {
            _logger.LogTrace($"HistoryToRabbitMQ Task End, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            CoordinateHistoryTask();
        }

        #endregion

        #region Channel

        private IModel CreateChannel(string brokerName, bool isPublish)
        {
            IModel channel = _connectionManager.CreateChannel(brokerName: _connectionSetting.BrokerName, isPublish: true);

            //Events
            SettingUpChannelEvents(channel);

            //Declare
            channel.ExchangeDeclare(_connectionSetting.ExchangeName, ExchangeType.Direct, true, false);

            //Confirm Mode
            channel.ConfirmSelect();

            return channel;
        }

        private void SettingUpChannelEvents(IModel channel)
        {
            channel.BasicReturn += (sender, eventArgs) =>
            {
                //直接留在history中，稍后会有人打扫
                _logger.LogWarning($"rabbitmq 没有 queue接受，留在 history 中：{DistributedQueueHistoryName} 。MessageType:{eventArgs.RoutingKey}");
            };

            channel.BasicRecoverOk += (sender, eventArgs) =>
            {
                //Log 记录
                _logger.LogInformation($"RabbitMQ Broker : {_connectionSetting.BrokerName} Recovery Ok.");
            };

            channel.CallbackException += (sender, eventArgs) =>
            {
                string detailMessage = "RabbitMQ Channel Exception:";

                if (eventArgs != null & eventArgs.Detail != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.AppendLine(detailMessage);

                    foreach (KeyValuePair<string, object> pair in eventArgs.Detail)
                    {
                        stringBuilder.AppendLine($"{pair.Key} : {pair.Value.ToString()}");
                    }

                    detailMessage = stringBuilder.ToString();
                }

                _logger.LogError($"RabbitMQ Broker : {_connectionSetting.BrokerName}, Detail:{detailMessage}");
            };

            channel.FlowControl += (sender, eventArgs) =>
            {
                _logger.LogInformation($"RabbitMQ Broker : {_connectionSetting.BrokerName}, Active: {eventArgs.Active}");
            };

            channel.ModelShutdown += (sender, eventArgs) =>
            {
                _logger.LogInformation($"RabbitMQ Broker : {_connectionSetting.BrokerName}, {eventArgs.ToString()}");
            };
        }
        
        private void DeclareEventType(IModel channel, EventMessageEntity message)
        {
            if (message == null)
            {
                return;
            }

            if (_eventDeclareDict.ContainsKey(message.Type))
            {
                return;
            }

            string queueName = EventTypeToQueueName(message.Type);
            string routingKey = EventTypeToRoutingKey(message.Type);

            //Queue
            channel.QueueDeclare(queueName, true, false, false);

            //Bind
            channel.QueueBind(queueName, _connectionSetting.ExchangeName, routingKey);

            _eventDeclareDict.TryAdd(message.Type, true);
        }

        private string RoutingKeyToEventType(string routingKey)
        {
            return routingKey;
        }

        private string EventTypeToRoutingKey(string eventType)
        {
            return eventType;
        }

        private string EventTypeToQueueName(string eventType)
        {
            return eventType;
        }

        #endregion

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
