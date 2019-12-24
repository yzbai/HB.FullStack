using HB.Framework.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.EventBus
{
    /// <summary>
    /// 单例启动
    /// </summary>
    internal class DefaultEventBus : IEventBus
    {
        private readonly IEventBusEngine _engine;
        private readonly IDictionary<string, EventSchema> _eventSchemaDict;

        public DefaultEventBus(IEventBusEngine eventBusEngine)
        {
            _engine = eventBusEngine;
            _eventSchemaDict = eventBusEngine.EventBusSettings.EventSchemas.ToDictionary(e => e.EventType);
        }

        /// <summary>
        /// PublishAsync
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.EventBus.EventBusException"></exception>
        public async Task PublishAsync(EventMessage eventMessage)
        {
            if (!EventMessage.IsValid(eventMessage))
            {
                throw new EventBusException($"not a valid event message : {SerializeUtil.ToJson(eventMessage)}");
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

            throw new EventBusException($"Not Found Matched EventSchema for EventType:{eventType}");
        }

        public void Close()
        {
            _engine.Close();
        }
    }
}
