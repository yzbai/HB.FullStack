using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Common;
using HB.Framework.EventBus.Abstractions;
using HB.Infrastructure.Redis;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HB.Infrastructure.RabbitMQ
{
    /// <summary>
    /// 一个EventType，对应一个线程的Consumer，用BasicQos来控制速度
    /// </summary>
    public class ConsumeTaskManager
    {
        private readonly string _eventType;
        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;
        private IRedisEngine _redis;

        private IEventHandler _handler;

        private IModel _channel;
        private string _consumeTag;

        public bool AutoRecovery { get; set; } = true;

        public ConsumeTaskManager(string eventType, IEventHandler eventHandler, RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IRedisEngine redis, ILogger logger)
        {
            _redis = redis;
            _eventType = eventType.ThrowIfNull(nameof(eventType));
            _handler = eventHandler.ThrowIfNull(nameof(eventHandler));
            _logger = logger.ThrowIfNull(nameof(logger));
            _connectionManager = connectionManager.ThrowIfNull(nameof(connectionManager));
            _connectionSetting = connectionSetting.ThrowIfNull(nameof(connectionSetting));

            Restart();
        }

        public void Cancel()
        {
            if (_channel != null && _channel.CloseReason == null && !string.IsNullOrEmpty(_consumeTag))
            {
                try
                {
                    _channel.BasicCancel(_consumeTag);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Basic Cancel Error. consumeTag : {_consumeTag}, message:{ex.Message}");
                }
            }

            _channel?.Close();
        }

        public void Restart()
        {
            Cancel();

            try
            {
                _channel = CreateChannel(_connectionSetting.BrokerName, false);

                string queueName = EventTypeToRabbitQueueName(_eventType);

                _channel.QueueDeclarePassive(queueName);

                _channel.BasicQos(0, _connectionSetting.ConsumePerTimeNumber, false);

                EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

                consumer.Received += (sender, eventArgs) =>
                {
                    EventMessageEntity entity = DataConverter.DeSerialize<EventMessageEntity>(eventArgs.Body);

                    //时间戳检测
                    if (CheckTimestamp(entity))
                    {
                        //防重检测
                        bool setted = _redis.KeySetIfNotExist(_connectionSetting.RedisInstanceName, entity.Id, expireSeconds: _connectionSetting.AliveSeconds);
                        
                        if (setted)
                        {
                            _handler.Handle(entity.JsonData);
                        }
                        else
                        {
                            _logger.LogWarning($"找到一个重复EventMessage, Type : {entity.Type}, Timestamp:{entity.Timestamp}, Data:{entity.JsonData}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"找到一个过期EventMessage, Type : {entity.Type}, Timestamp:{entity.Timestamp}, Data:{entity.JsonData}");
                    }

                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                };

                _consumeTag = _channel.BasicConsume(queueName, false, consumer);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"在Consume RabbitMQ {_connectionSetting.BrokerName} 中，Exceptions: {ex.Message}");
            }
            finally
            {
                Cancel();

                if (AutoRecovery)
                {
                    Restart();
                }
            }
        }

        private bool CheckTimestamp(EventMessageEntity entity)
        {
            long seconds = DataConverter.CurrentTimestampSeconds() - entity.Timestamp;

            if (seconds <= _connectionSetting.AliveSeconds)
            {
                return true;
            }

            _logger.LogCritical($"找到一个过期的EventMessage, Type : {entity.Type}, Timestamp:{entity.Timestamp}, Data:{entity.JsonData}");

            return false;
        }

        private IModel CreateChannel(string brokerName, bool isPublish)
        {
            IModel channel = _connectionManager.CreateChannel(brokerName: _connectionSetting.BrokerName, isPublish: true);

            //Events
            SettingUpChannelEvents(channel);

            return channel;
        }

        private void SettingUpChannelEvents(IModel channel)
        {
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
        }

        private static string EventTypeToRabbitQueueName(string eventType)
        {
            return eventType;
        }
    }
}