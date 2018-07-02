using HB.Framework.EventBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Component.CentralizedLogger
{
    public sealed class CentralizedLoggerProvider : ILoggerProvider
    {
        private CentralizedLoggerOptions _options;
        private IEventBus _eventBus;
        private CentralizedLoggerProcessor _processor;

        private Func<string, LogLevel, bool> _filter;
        private readonly ConcurrentDictionary<string, CentralizedLogger> _loggers;


        public CentralizedLoggerProvider(IOptions<CentralizedLoggerOptions> options, IEventBus eventBus, CentralizedLoggerProcessor processor)
        {
            _eventBus = eventBus;
            _options = options.Value;
            _processor = processor;
            _loggers = new ConcurrentDictionary<string, CentralizedLogger>();
            _filter = getFilter(_options.LogLevel);

            addEventInfoToEventBus();
        }

        private void addEventInfoToEventBus()
        {
            _eventBus.AddEventConfig(new EventConfig() {
                MessageQueueServerName = _options.MessageQueueServerName,
                EventName = _options.LogEventName
            });
        }

        private static Func<string, LogLevel, bool> getFilter(string logLevelString)
        {
            if (!Enum.TryParse(logLevelString, out LogLevel level))
            {
                level = LogLevel.Information;
            }

            return (n, l) => l >= level;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => {
                return new CentralizedLogger(_options.HostName, name, _filter, _processor);
            });
        }

        public void Dispose()
        {
            _processor.Dispose();
        }
    }
}
