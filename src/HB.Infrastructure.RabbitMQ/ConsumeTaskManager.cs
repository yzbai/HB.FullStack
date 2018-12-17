using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.EventBus.Abstractions;
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
        private string _eventType;
        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;

        private IEventHandler _handler;

        private IModel _channel;
        private string _consumeTag;

        public ConsumeTaskManager(string eventType, RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, ILogger logger)
        {
            _eventType = eventType;
            _logger = logger;
            _connectionManager = connectionManager;
            _connectionSetting = connectionSetting;

            Restart();
        }

        public void Cancel()
        {
            if (_channel != null && _channel.CloseReason == null && !string.IsNullOrEmpty(_consumeTag))
            {
                _channel.BasicCancel(_consumeTag);
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
                    byte[] data = eventArgs.Body;

                    _handler.Handle(data);

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
                _channel?.Close();
            }
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

        private string EventTypeToRabbitQueueName(string eventType)
        {
            return eventType;
        }
    }
}