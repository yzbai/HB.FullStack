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
    public class PublishTaskManager : DynamicTaskManager
    {
        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _distributedQueue;
                
        //EventMessageEntity.Type
        private ConcurrentDictionary<string, bool> _eventDeclareDict = new ConcurrentDictionary<string, bool>();

        private ulong _inCommingCount = 0;
        private object _taskNodesLocker = new object();
        private LinkedList<TaskNode> _taskNodes = new LinkedList<TaskNode>();

        public PublishTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IDistributedQueue distributedQueue, ILogger logger)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _distributedQueue = distributedQueue;
            _connectionSetting = connectionSetting;

            AddTaskAndStart(InitialTaskNumber);
        }

        private void TaskProcedure()
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
                    IDistributedQueueResult queueResult = _distributedQueue.PopAndPush<EventMessageEntity>(fromQueueName: DistributedQueueName, toQueueName: DistributedHistoryQueueName);

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

                OnTaskFinished();
            }
        }

        private ulong PerThreadFacingCount
        {
            get { return _connectionSetting.PerThreadFacingEventCount; }
        }

        public void NotifyInComming()
        {
            //管理
            //控制Task的数量

            _inCommingCount++;

            if (_inCommingCount > PerThreadFacingCount)
            {
                CoordinateTaskNumber();
                _inCommingCount = 0;
            }
        }

        private int InitialTaskNumber
        {
            get
            {
                //根据初始队列长度，确定线程数量
                ulong length = _distributedQueue.Length(queueName: DistributedQueueName);

                if (length == 0)
                {
                    return 0;
                }

                int taskNumber = (int)(length / PerThreadFacingCount) + 1;

                return taskNumber;
            }

        }

        private void AddTaskAndStart(int count)
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

        private void CoordinateTaskNumber()
        {
            lock(_taskNodesLocker)
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

                    AddTaskAndStart(toAddNumber);
                }
            }
        }

        private void OnTaskFinished()
        {
            _logger.LogTrace($"PublishToRabbitMQ Task End, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            CoordinateTaskNumber();
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
                _logger.LogWarning($"rabbitmq 没有 queue接受，留在 history 中 。MessageType:{eventArgs.RoutingKey}");
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
    }
}
