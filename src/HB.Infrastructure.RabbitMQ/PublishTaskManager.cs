using System;
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
    internal class TaskNode
    {
        public Task Task { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }
    }

    /// <summary>
    /// 每一个PublishWokerManager，负责一个broker的工作，自己决定工作量的大小
    /// eventType = routingkey = queuename
    /// </summary>
    public class PublishTaskManager
    {
        private const ulong PER_THREAD_FACING = 100; 

        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _distributedQueue;

        private LinkedList<TaskNode> _taskNodes = new LinkedList<TaskNode>();

        //EventMessageEntity.Type
        private ConcurrentDictionary<string, bool> _eventDeclareDict = new ConcurrentDictionary<string, bool>();

        public PublishTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IDistributedQueue distributedQueue, ILogger logger)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _distributedQueue = distributedQueue;
            _connectionSetting = connectionSetting;

            for (int i = 0; i < InitialTaskNumber(); ++i)
            {
                CancellationTokenSource cs = new CancellationTokenSource();

                Task task = Task.Factory.StartNew(PublishToRabbitMQ, cs.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                TaskNode taskNode = new TaskNode() { Task = task, CancellationTokenSource = cs };

                _taskNodes.AddLast(taskNode);
            }
        }

        public void NotifyPublishComming()
        {
            //管理
            //控制Task的数量
        }

        //await前后线程不同。想让前后线程一致。Task里避免用await，用wait()
        //per thread per channel
        private void PublishToRabbitMQ()
        {
            _logger.LogTrace($"Enter Thread : {Thread.CurrentThread.ManagedThreadId}");
            IModel channel = null;

            LinkedList<string> confirmEventIdList = new LinkedList<string>();
            ulong firstListNodeSeqno = 1;

            
            
            try
            {
                channel = CreateChannel(brokerName: _connectionSetting.BrokerName, isPublish: true);

                channel.BasicAcks += (sender, eventArgs) =>
                {
                    List<string> ids = new List<string>();

                    if (eventArgs.Multiple)
                    {

                    }
                    else
                    {
                        
                    }

                    tempEventIdDicts.Remove()

                    //从brokerName_history删除
                };

                channel.BasicNacks += (sender, eventArgs) =>
                {
                    //重新放入队列中,比较靠前
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

                    ulong sequenceNumber = channel.NextPublishSeqNo;

                    channel.BasicPublish(_connectionSetting.ExchangeName, EventTypeToRoutingKey(eventEntity.Type), true, basicProperties, eventEntity.Body);

                    eventIdList.AddLast(eventEntity.Id);
                    //tempEventIdList.Add(eventEntity.Id);
                    //tempEventIdDicts.Add(sequenceNumber, eventEntity.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"在PublishToRabbitMQ {_connectionSetting.BrokerName} 中，Thread Id : {Thread.CurrentThread.ManagedThreadId}, Exceptions: {ex.Message}");
            }
            finally
            {
                channel?.Close();
            }
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

        private string DistributedQueueName
        {
            get { return _connectionSetting.BrokerName; }
        }

        private string DistributedQueueHistoryName
        {
            get { return _connectionSetting.BrokerName + "_History"; }
        }

        private int InitialTaskNumber()
        {
            //根据初始队列长度，确定线程数量
            ulong length = _distributedQueue.Length(queueName: DistributedQueueName);

            if (length == 0)
            {
                return 0;
            }

            int taskNumber = (int)(length / PER_THREAD_FACING) + 1;

            return taskNumber;

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
    }
}
