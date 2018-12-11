using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    /// </summary>
    public class PublishTaskManager
    {
        private const ulong PER_THREAD_FACING = 100; 

        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _queue;

        private LinkedList<TaskNode> _taskNodes;

        public PublishTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IDistributedQueue queue, ILogger logger)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _queue = queue;
            _connectionSetting = connectionSetting;

            _taskNodes = new LinkedList<TaskNode>();
            
            for (int i = 0; i < InitialTaskNumber(); ++i)
            {
                CancellationTokenSource cs = new CancellationTokenSource();

                Task task = Task.Factory.StartNew(PublishToRabbitMQ, cs.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                TaskNode taskNode = new TaskNode() { Task = task, CancellationTokenSource = cs };

                _taskNodes.AddLast(taskNode);
            }
        }

        private string QueueName
        {
            get { return _connectionSetting.BrokerName; }
        }

        private string QueueHistoryName
        {
            get { return _connectionSetting.BrokerName + "_History"; }
        }

        private int InitialTaskNumber()
        {
            //根据初始队列长度，确定线程数量
            ulong length = _queue.Length(queueName: QueueName);

            if (length == 0)
            {
                return 0;
            }

            int taskNumber = (int)(length / PER_THREAD_FACING) + 1;

            return taskNumber;

        }

        //await前后线程不同。想让前后线程一致。Task里避免用await，用wait()
        //per thread per channel
        private void PublishToRabbitMQ()
        {
            IModel channel = null;

            try
            {
                channel = CreateChannel(brokerName: _connectionSetting.BrokerName, isPublish: true);

                while (true)
                {
                    //获取数据
                    IDistributedQueueResult<EventMessage> queueResult = _queue.PopAndPush<EventMessage>(fromQueueName: QueueName, toQueueName: QueueHistoryName);

                    if (!queueResult.IsSucceeded() || !EventMessage.IsValid(queueResult.Data))
                    {
                        //可能性1：Queue挂掉，取不到；可能性2：Queue已空
                        //退出循环

                        bool allAcks = channel.WaitForConfirms(TimeSpan.FromSeconds(_connectionSetting.MaxSecondsWaitForConfirms), out bool timedOut);

                        if (allAcks && !timedOut)
                        {
                            _logger.LogDebug($"Task will end, no in queue : {QueueName}, and all good.");
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

                    //确保Channel 可用, 不可用，再create一个
                    if (channel ==null || channel.CloseReason != null)
                    {
                        _logger.LogWarning($"Channel for broker: {_connectionSetting.BrokerName}, has closed, reason:{channel?.CloseReason.ToString()}");

                        channel = CreateChannel(brokerName: _connectionSetting.BrokerName, isPublish: true);
                    }

                    //publish
                    IBasicProperties basicProperties = channel.CreateBasicProperties();
                    basicProperties.DeliveryMode = 2;

                    ulong sequenceNumber = channel.NextPublishSeqNo;




                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"在PublishToRabbitMQ中，Thread Id : {Thread.CurrentThread.ManagedThreadId}, Exceptions: {ex.Message}");
            }
            finally
            {
                channel?.Close();
            }
        }

        private IModel CreateChannel(string brokerName, bool isPublish)
        {
            IModel channel = _connectionManager.CreateChannel(brokerName: _connectionSetting.BrokerName, isPublish: true);

            SettingUpChannelEvents(channel);

            channel.ExchangeDeclare(_connectionSetting.ExchangeName, ExchangeType.Direct, true, false);
            //channel.QueueDeclare(_)

            channel.ConfirmSelect();

            return channel;
        }

        private static void SettingUpChannelEvents(IModel channel)
        {
            channel.BasicReturn += (sender, eventArgs) =>
            {
                //没有人听，返回来，重新放入
                Console.WriteLine($"broker return : {eventArgs.Exchange} : {eventArgs.RoutingKey}");
            };

            channel.BasicAcks += (sender, eventArgs) =>
            {
                Console.WriteLine($"broker ack : {eventArgs.DeliveryTag}");
                //从brokerName_history删除
            };

            channel.BasicNacks += (sender, eventArgs) =>
            {
                //重新放入队列中,比较靠前
                Console.WriteLine($"broker nack : {eventArgs.DeliveryTag}");
            };

            channel.BasicRecoverOk += (sender, eventArgs) =>
            {
                //重新放入队列中，比较靠前
                //Log 记录
            };

            channel.CallbackException += (sender, eventArgs) =>
            {
                //Re create channel
            };

            channel.FlowControl += (sender, eventArgs) =>
            {

            };

            channel.ModelShutdown += (sender, eventArgs) =>
            {

            };
        }

        public void NotifyPublishComming()
        {
            //管理
            //控制Task的数量
        }
    }
}
