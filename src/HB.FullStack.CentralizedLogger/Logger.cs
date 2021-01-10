using HB.Framework.Common;
using HB.Framework.EventBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.CentralizedLogger
{

    /// <summary>
    /// This is Cheap
    /// </summary>
    public class Logger : ILogger
    {
        private string _host;
        private string _name;
        private readonly Func<string, LogLevel, bool> _filter;
        private LoggerProcessor _processor;

        public Logger(string host, string name, Func<string, LogLevel, bool> filter, LoggerProcessor processor)
        {
            _host = host;
            _name = name;
            _filter = filter;
            _processor = processor;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
            {
                return false;
            }

            return _filter(_name, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                LogEntity logEntity = new LogEntity {
                    DateTime = DateTime.Now,
                    HostName = _host,
                    LoggerName = _name,
                    LogLevel = logLevel.ToString(),
                    EventId = eventId,
                    Message = message,
                    Exception = exception
                };

                _processor.EnqueLogEntity(logEntity);
            }
        }
    }
}
