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
    public class DefaultEventBus : IEventBus
    {
        private readonly IEventBusEngine _engine;
        private readonly IDictionary<string, EventSchema> _eventSchemaDict;

        public DefaultEventBus(IEventBusEngine eventBusEngine)
        {
            _engine = eventBusEngine;
            _eventSchemaDict = eventBusEngine.EventBusSettings.EventSchemas.ToDictionary(e => e.EventName);
        }

        public async Task PublishAsync(string eventName, string jsonData)
        {
            await _engine.PublishAsync(GetBrokerName(eventName), eventName, jsonData).ConfigureAwait(false);
        }

        public void StartHandle(string eventName)
        {
            _engine.StartHandle(eventName);
        }

        public void Subscribe(string eventName, IEventHandler handler)
        {
            _engine.SubscribeHandler(brokerName: GetBrokerName(eventName), eventName: eventName, eventHandler: handler);
        }

        public async Task UnSubscribeAsync(string eventName)
        {
            await _engine.UnSubscribeHandlerAsync(eventyName: eventName).ConfigureAwait(false);
        }

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