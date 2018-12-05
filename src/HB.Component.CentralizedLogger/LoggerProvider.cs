using HB.Framework.EventBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Component.CentralizedLogger
{
    public sealed class LoggerProvider : ILoggerProvider
    {
        private LoggerOptions _options;
        private IEventBus _eventBus;
        private LoggerProcessor _processor;

        private readonly Func<string, LogLevel, bool> _filter;
        private readonly ConcurrentDictionary<string, Logger> _loggers;


        public LoggerProvider(IOptions<LoggerOptions> options, IEventBus eventBus, LoggerProcessor processor)
        {
            _eventBus = eventBus;
            _options = options.Value;
            _processor = processor;
            _loggers = new ConcurrentDictionary<string, Logger>();
            _filter = GetFilter(_options.LogLevel);

            AddEventInfoToEventBus();
        }

        private void AddEventInfoToEventBus()
        {
            _eventBus.RegisterEvent(new EventConfig() {
                ServerName = _options.ServerName,
                EventName = _options.LogEventName
            });
        }

        private static Func<string, LogLevel, bool> GetFilter(string logLevelString)
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
                return new Logger(_options.HostName, name, _filter, _processor);
            });
        }

        public void Dispose()
        {
            _processor.Dispose();
        }
    }
}
