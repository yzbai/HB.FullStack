using HB.Framework.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using HB.Framework.EventBus.Abstractions;

namespace HB.Framework.EventBus
{
    /// <summary>
    /// 单例启动
    /// </summary>
    public class DefaultEventBus : IEventBus
    {
        private readonly EventBusOptions _options;
        private readonly IEventBusEngine _engine;
        private readonly ILogger _logger;

        public DefaultEventBus(IEventBusEngine eventBusEngine, IOptions<EventBusOptions> options, ILogger<DefaultEventBus> logger)
        {
            _options = options.Value;
            _engine = eventBusEngine;
            _logger = logger;
        }
      
        public async Task<bool> PublishAsync(EventMessage eventMessage)
        {
            if (!EventMessage.IsValid(eventMessage))
            {
                Exception ex = new ArgumentException("不符合要求", nameof(eventMessage));
                _logger.LogCritical(ex, "用于Publish的eventMessage不符合要求");

                throw ex;
            }

            return await _engine.PublishAsync(GetBrokerName(eventMessage.Type), eventMessage).ConfigureAwait(false);
        }

        public void StartHandle(string eventType)
        {
            _engine.StartHandle(eventType);
        }

        public void Subscribe(IEventHandler handler)
        {
            handler.ThrowIfNull(nameof(handler));

            handler.EventType.ThrowIfNull(nameof(handler.EventType));

            _engine.SubscribeHandler(brokerName: GetBrokerName(handler.EventType), eventHandler: handler);
        }

        public void UnSubscribe(string eventType)
        {
            eventType.ThrowIfNull(nameof(eventType));

            _engine.UnSubscribeHandler(eventyType: eventType);
        }

        private string GetBrokerName(string eventType)
        {
            string brokerName = _options.GetEventSchema(eventType)?.BrokerName;

            if (string.IsNullOrEmpty(brokerName))
            {
                Exception ex = new Exception("配置中没有找到对应主题事件的Broker");

                _logger.LogCritical(ex, $"没有Topic对应的BrokerName， eventType：{eventType}");

                throw ex;
            }

            return brokerName;
        }
    }
}
