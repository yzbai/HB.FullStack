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
    /// <summary>
    /// 一个EventType，对应一个线程的Consumer，用BasicQos来控制速度
    /// </summary>
    public class ConsumeTaskManager
    {
        private string _eventType;
        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;

        //eventType : Handler
        private IEventHandler _handler;

        private Task _task;
        private CancellationTokenSource _cts;

        public bool AutoReStartOver { get; set; } = true;

        public ConsumeTaskManager(string eventType, RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, ILogger logger)
        {
            _eventType = eventType;
            _logger = logger;
            _connectionManager = connectionManager;
            _connectionSetting = connectionSetting;

            StartNewTask();
        }

        private void StartNewTask()
        {
            _cts = new CancellationTokenSource();
            _task = Task.Factory.StartNew(TaskProcedure, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void TaskProcedure()
        {
            _logger.LogTrace($"Consum Task Start, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            IModel channel = null;

            try
            {
                channel = CreateChannel(_connectionSetting.BrokerName, false);
                channel.BasicQos(0, _connectionSetting.ConsumePerTimeNumber, false);
                
                string queueName = EventTypeToQueueName()

                while(true)
                {

                }
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, $"在Consume RabbitMQ {_connectionSetting.BrokerName} 中，Thread Id : {Thread.CurrentThread.ManagedThreadId}, Exceptions: {ex.Message}");
            }
            finally
            {
                channel?.Close();

                OnTaskFinished();
            }
        }

        private void OnTaskFinished()
        {
            if (AutoReStartOver)
            {
                StartNewTask();
            }
        }

        public void ReStart()
        {
            if (!_task.IsCompleted)
            {
                _cts.Cancel();
            }

            StartNewTask();
        }

        public bool AddEventHandler(string eventType, IEventHandler eventHandler)
        {
            lock(_handlersLocker)
            {
                if (_handlers.ContainsKey(eventType))
                {
                    return false;
                }

                _handlers.Add(eventType, eventHandler);

                return true;
            }
        }

        public bool UpdateEventHandler(string eventType, IEventHandler eventHandler)
        {
            lock(_handlersLocker)
            {
                if (!_handlers.ContainsKey(eventType))
                {
                    return false;
                }

                _handlers[eventType] = eventHandler;

                return true;
            }
        }

        public bool RemoveEventHandler(string eventyType, string handlerId)
        {
            lock(_handlersLocker)
            {
                if (!_handlers.ContainsKey(eventyType))
                {
                    return false;
                }

                _handlers.Remove(eventyType);

                return true;
            }
        }

        #region Channel

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

        internal void Cancel()
        {
            throw new NotImplementedException();
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