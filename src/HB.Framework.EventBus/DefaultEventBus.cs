using HB.Framework.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    internal class DefaultEventBus : IEventBus
    {
        private readonly IEventBusEngine _engine;
        private readonly ILogger _logger;
        private readonly IDictionary<string, EventSchema> _eventSchemaDict;

        public DefaultEventBus(IEventBusEngine eventBusEngine, ILogger<DefaultEventBus> logger)
        {
            _engine = eventBusEngine;
            _logger = logger;
            _eventSchemaDict = eventBusEngine.EventBusSettings.EventSchemas.ToDictionary(e => e.EventType);
        }

        /// <summary>
        /// PublishAsync
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.EventBus.EventBusException">
        /// </exception>
        public async Task PublishAsync(EventMessage eventMessage)
        {
            if (!EventMessage.IsValid(eventMessage))
            {
                EventBusException ex = new EventBusException($"not a valid event message : {SerializeUtil.ToJson(eventMessage)}");
                _logger.LogException(ex, null, LogLevel.Critical);

                throw ex;
            }

            await _engine.PublishAsync(GetBrokerName(eventMessage.Type), eventMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// StartHandle
        /// </summary>
        /// <param name="eventType"></param>
        /// <exception cref="HB.Framework.EventBus.EventBusException"></exception>
        public void StartHandle(string eventType)
        {
            _engine.StartHandle(eventType);
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        /// <exception cref="HB.Framework.EventBus.EventBusException"></exception>
        public void Subscribe(string eventType, IEventHandler handler)
        {
            eventType.ThrowIfNullOrEmpty(nameof(eventType));
            handler.ThrowIfNull(nameof(handler));

            _engine.SubscribeHandler(brokerName: GetBrokerName(eventType), eventType: eventType, eventHandler: handler);
        }

        /// <summary>
        /// UnSubscribe
        /// </summary>
        /// <param name="eventType"></param>
        /// <exception cref="HB.Framework.EventBus.EventBusException"></exception>
        public void UnSubscribe(string eventType)
        {
            eventType.ThrowIfNull(nameof(eventType));

            _engine.UnSubscribeHandler(eventyType: eventType);
        }

        /// <summary>
        /// GetBrokerName
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.EventBus.EventBusException"></exception>
        private string GetBrokerName(string eventType)
        {
            if (_eventSchemaDict.TryGetValue(eventType, out EventSchema eventSchema))
            {
                return eventSchema.BrokerName;
            }

            EventBusException ex = new EventBusException($"Not Found Matched EventSchema for EventType:{eventType}");

            _logger.LogException(ex, null, LogLevel.Critical);

            throw ex;
        }

        public void Close()
        {
            _engine.Close();
        }
    }
}
