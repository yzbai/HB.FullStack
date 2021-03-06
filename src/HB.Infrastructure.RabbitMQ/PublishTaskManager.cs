﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using HB.Infrastructure.Redis.Direct;

namespace HB.Infrastructure.RabbitMQ
{
    /// <summary>
    /// 每一个PublishWokerManager，负责一个broker的工作，自己决定工作量的大小
    /// eventType = routingkey = queuename
    /// </summary>
    public class PublishTaskManager : RabbitMQAndDistributedQueueDynamicTaskManager
    {
        //EventMessageEntity.Type
        private readonly ConcurrentDictionary<string, bool> _eventDeclareDict = new ConcurrentDictionary<string, bool>();

        public PublishTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IRedisDatabase redis, ILogger logger)
            : base(connectionSetting, connectionManager, redis, logger) { }

        protected override void TaskProcedure()
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
                    _logger.LogTrace($"Ack: {eventArgs.DeliveryTag}, Multiple:{eventArgs.Multiple}");

                    List<string> deleteIds = new List<string>();

                    lock (confirmEventIdListLocker)
                    {
                        ulong seqno = eventArgs.DeliveryTag;
                        int index = (int)(seqno - fs);

                        if (index >= 0 && index < confirmEventIdList.Count)
                        {
                            if (eventArgs.Multiple)
                            {
                                for (int i = 0; i < index; i++)
                                {
                                    if (confirmEventIdList[i] != null)
                                    {
                                        deleteIds.Add(confirmEventIdList[i]);
                                        confirmEventIdList[i] = null;
                                    }
                                }
                            }
                            else
                            {
                                deleteIds.Add(confirmEventIdList[index]);
                                confirmEventIdList[index] = null;
                            }
                        }

                        // 收缩,每100次，收缩一次
                        if (seqno % 100 == 0 && confirmEventIdList.Count > 100)
                        {
                            _logger.LogTrace($"开始收缩：TheadID:{Thread.CurrentThread.ManagedThreadId}, count:{confirmEventIdList.Count}, fs:{fs}");

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

                            fs += (ulong)nextIndex;

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

                    _redis.HashSetInt(redisInstanceName: _connectionSetting.RedisInstanceName, hashName: DistributedConfirmIdHashName, fields: deleteIds, values: valueList);
                };

                channel.BasicNacks += (sender, eventArgs) =>
                {
                    _logger.LogWarning($"NooooAck: {eventArgs.DeliveryTag}, Multiple:{eventArgs.Multiple}");

                    //List<string> deleteIds = new List<string>();

                    ////那就在history里待着吧，等待回收
                    lock (confirmEventIdListLocker)
                    {
                        ulong seqno = eventArgs.DeliveryTag;
                        int index = (int)(seqno - fs);

                        if (eventArgs.Multiple)
                        {
                            for (int i = 0; i < index; i++)
                            {
                                if (confirmEventIdList[i] != null)
                                {
                                    //deleteIds.Add(confirmEventIdList[i]);
                                    confirmEventIdList[i] = null;
                                }
                            }
                        }
                        else
                        {
                            //deleteIds.Add(confirmEventIdList[index]);
                            confirmEventIdList[index] = null;
                        }
                    }

                    //将deleteIds放入 DistributedConfirmEventIdSet

                    //List<int> valueList = new List<int>();

                    //foreach (string id in deleteIds)
                    //{
                    //    valueList.Add(0);
                    //}

                    //_redis.HashSetInt(redisInstanceName: _connectionSetting.RedisInstanceName, hashName: DistributedConfirmIdHashName, fields: deleteIds, values: valueList);
                };

                while (true)
                {
                    //获取数据.用同步方法，不能用await，避免前后线程不一致
                    
                    EventMessageEntity eventEntity = _redis.PopAndPush<EventMessageEntity>(
                        redisInstanceName: _connectionSetting.RedisInstanceName, 
                        fromQueueName: DistributedQueueName, 
                        toQueueName: DistributedHistoryQueueName
                    );

                    //没有数据，queue为空，直接推出,结束Thread
                    if (eventEntity == null)
                    {
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

                    //Channel 不可用，直接结束Thread
                    if (channel == null || channel.CloseReason != null)
                    {
                        _logger.LogWarning($"Channel for broker: {_connectionSetting.BrokerName}, has closed, reason:{channel?.CloseReason.ToString()}");

                        break;
                    }

                    //Declare Queue & Binding
                    DeclareRabbitMQ(channel, eventEntity);
                    
                    //publish
                    IBasicProperties basicProperties = channel.CreateBasicProperties();
                    basicProperties.DeliveryMode = 2;

                    channel.BasicPublish(
                        _connectionSetting.ExchangeName, 
                        EventTypeToRabbitRoutingKey(eventEntity.Type), 
                        true, 
                        basicProperties, 
                        JsonUtil.Serialize(eventEntity)
                    );

                    //_logger.LogTrace($"event published. Type:{eventEntity.Type}, Data:{eventEntity.JsonData}, Id:{eventEntity.Id}, Timestamp:{eventEntity.Timestamp}");

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
                bool? allAcks = channel?.WaitForConfirms(TimeSpan.FromSeconds(_connectionSetting.MaxSecondsWaitForConfirms));

                _logger.LogTrace($"Tread ID {Thread.CurrentThread.ManagedThreadId} de channel waiting end. begin to close. allAkcs? {allAcks}");

                channel?.Close();

                OnTaskFinished();
            }
        }

        protected override ulong PerThreadFacingCount()
        {
            return _connectionSetting.PerThreadFacingEventCount;
        }

        protected override string CurrentWorkloadQueueName()
        {
            return DistributedQueueName;
        }

        protected override int MaxWorkerThread()
        {
            return _connectionSetting.MaxPublishWorkerThread;
        }

        #region Channel

        private IModel CreateChannel(string brokerName, bool isPublish)
        {
            IModel channel = _connectionManager.CreateChannel(brokerName: _connectionSetting.BrokerName, isPublish: true);

            _logger.LogTrace($"Channel Created. BrokerName :{brokerName}, isPublishChannel:{isPublish}");

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
        
        private void DeclareRabbitMQ(IModel channel, EventMessageEntity message)
        {
            if (message == null)
            {
                return;
            }

            if (_eventDeclareDict.ContainsKey(message.Type))
            {
                return;
            }

            string queueName = EventTypeToRabbitQueueName(message.Type);
            string routingKey = EventTypeToRabbitRoutingKey(message.Type);

            //Queue
            channel.QueueDeclare(queueName, true, false, false);

            //Bind
            channel.QueueBind(queueName, _connectionSetting.ExchangeName, routingKey);

            _eventDeclareDict.TryAdd(message.Type, true);
        }

        private static string RabbitRoutingKeyToEventType(string routingKey)
        {
            return routingKey;
        }

        private static string EventTypeToRabbitRoutingKey(string eventType)
        {
            return eventType;
        }

        private static string EventTypeToRabbitQueueName(string eventType)
        {
            return eventType;
        }

        

        #endregion
    }
}
