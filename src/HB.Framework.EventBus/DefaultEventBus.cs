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
        private IEnumerable<IEventHandler> _eventHandlers;
        private bool _handled;

        public DefaultEventBus(IEventBusEngine eventBusEngine, IEnumerable<IEventHandler> eventHandlers, IOptions<EventBusOptions> options, ILogger<DefaultEventBus> logger)
        {
            _options = options.Value;
            _engine = eventBusEngine;
            _logger = logger;
            _eventHandlers = eventHandlers;
            _handled = false;
        }

        public void Handle()
        {
            if (_handled)
            {
                return;
            }


            if (_eventHandlers == null)
            {
                return;
            }
 
            foreach (var handler in _eventHandlers)
            {
                EventHandlerConfig handlerConfig = handler.GetConfig();

                if (handlerConfig != null)
                {
                    if (string.IsNullOrWhiteSpace(handlerConfig.SubscribeGroup))
                    {
                        handlerConfig.SubscribeGroup = _options.SubscribeConfig.DefaultSubscribeGroup;
                    }

                    _engine.SubscribeAndConsume(handlerConfig.ServerName, handlerConfig.SubscribeGroup, handlerConfig.EventName, handler);
                }
            }

            _handled = true;
        }

        public void RegisterEvent(EventConfig eventConfig)
        {
            if (eventConfig == null)
            {
                throw new ArgumentNullException("eventConfig");
            }

            if (_options.PublishConfig == null)
            {
                _options.PublishConfig = new PublishConfig();
            }

            if (_options.PublishConfig.Events == null)
            {
                _options.PublishConfig.Events = new List<EventConfig>();
            }

            _options.PublishConfig.Events.Add(eventConfig);
        }

        public Task<PublishResult> Publish(string eventName, string jsonString)
        {
            EventConfig eventConfig = GetEventConfiguration(eventName);

            if (eventConfig == null)
            {
                _logger.LogCritical("Event :{0} dot not have configuration.", eventName);
                return null;
            }

            return TaskRetry.Retry<PublishResult>(
                _options.PublishConfig.RetryCount,
                () => _engine.PublishString(eventConfig.ServerName, eventConfig.EventName, jsonString),
                (ret, ex) =>
                {
                    _logger.LogCritical(ex.Message);
                });
        }

        private EventConfig GetEventConfiguration(string eventName)
        {
            return _options.PublishConfig?.Events?.FirstOrDefault(e => e.EventName.Equals(eventName));
        }
    }
}
