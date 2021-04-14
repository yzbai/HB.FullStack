using HB.FullStack.EventBus.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.FullStack.EventBus
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
            _eventSchemaDict = eventBusEngine.EventBusSettings.EventSchemas.ToDictionary(e => e.EventName);
        }

        /// <summary>
        /// PublishAsync
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <returns></returns>
        /// <exception cref="EventBusException"></exception>
        public async Task PublishAsync(string eventName, string jsonData)
        {
            await _engine.PublishAsync(GetBrokerName(eventName), eventName, jsonData).ConfigureAwait(false);
        }

        /// <summary>
        /// StartHandle
        /// </summary>
        /// <param name="eventType"></param>
        /// <exception cref="EventBusException"></exception>
        public void StartHandle(string eventType)
        {
            _engine.StartHandle(eventType);
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        /// <exception cref="EventBusException"></exception>
        public void Subscribe(string eventType, IEventHandler handler)
        {
            _engine.SubscribeHandler(brokerName: GetBrokerName(eventType), eventName: eventType, eventHandler: handler);
        }

        /// <summary>
        /// UnSubscribe
        /// </summary>
        /// <param name="eventType"></param>
        /// <exception cref="EventBusException"></exception>
        public async Task UnSubscribeAsync(string eventType)
        {
            await _engine.UnSubscribeHandlerAsync(eventyName: eventType).ConfigureAwait(false);
        }

        /// <summary>
        /// GetBrokerName
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        /// <exception cref="EventBusException"></exception>
        private string GetBrokerName(string eventName)
        {
            if (_eventSchemaDict.TryGetValue(eventName, out EventSchema? eventSchema))
            {
                return eventSchema.BrokerName;
            }

            throw Exceptions.SettingsError(eventName: eventName, cause: "Not Found Matched EventSchema for EventType");
        }

        public void Close()
        {
            _engine.Close();
        }
    }
}
