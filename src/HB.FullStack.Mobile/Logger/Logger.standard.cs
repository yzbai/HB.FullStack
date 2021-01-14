using HB.FullStack.Mobile.Platforms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Mobile.Logger
{
    public class Logger : ILogger
    {
        private readonly IPlatformLoggerImpl? _impl;
        private readonly LogLevel _minLevel;

        public Logger(IPlatformLoggerImpl? impl, LogLevel logLevel)
        {
            _impl = impl;
            _minLevel = logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message = formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Trace:
                    _impl?.Trace(message);
                    break;
                case LogLevel.Debug:
                    _impl?.Debug(message);
                    break;
                case LogLevel.Information:
                    _impl?.Info(message);
                    break;
                case LogLevel.Warning:
                    _impl?.Warn(message);
                    break;
                case LogLevel.Error:
                    _impl?.Error(message);
                    break;
                case LogLevel.Critical:
                    _impl?.Wtf(message);
                    break;
                case LogLevel.None:
                    break;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minLevel;
        }

        public IDisposable? BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
