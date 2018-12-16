using System;
using System.Collections.Generic;
using HB.Framework.DistributedQueue;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace HB.Infrastructure.RabbitMQ
{
    /// <summary>
    /// 一个RabbitMQ的Queue，对应一个线程的Consumer，用BasicQos来控制速度
    /// </summary>
    public class ConsumeTaskManager
    {
        private ILogger _logger;
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _distributedQueue;

        //eventType : Handler
        private IDictionary<string, IEventHandler> _handlers;

        public ConsumeTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IDistributedQueue distributedQueue, ILogger logger)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _distributedQueue = distributedQueue;
            _connectionSetting = connectionSetting;

            _handlers = new Dictionary<string, IEventHandler>();
        }

        public void AddEventHandler(string eventType, IEventHandler eventHandler)
        {
            //_handlers
        }

        public void RemoveEventHandler(string eventyType, string handlerId)
        {
            
        }
    }
}