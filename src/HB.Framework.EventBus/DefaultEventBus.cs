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
        private EventBusOptions _options;
        private IEventBusEngine _engine;
        private ILogger _logger;

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

        public void Subscribe(string eventType, IEventHandler handler)
        {
            eventType.ThrowIfNull(nameof(eventType));

            handler.ThrowIfNull(nameof(handler));

            _engine.SubscribeHandler(brokerName: GetBrokerName(eventType), eventType: eventType, eventHandler: handler);
        }

        public void UnSubscribe(string eventType)
        {
            eventType.ThrowIfNull(nameof(eventType));

            _engine.UnSubscribeHandler(eventyType: eventType);
        }

        private string GetBrokerName(string eventType)
        {
            string brokerName = _options.GetTopicSchema(eventType)?.BrokerName;

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
