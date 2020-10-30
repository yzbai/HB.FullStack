using HB.Framework.Client.Platforms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HB.Framework.Client.Services
{
    public class PlatformLogger : ILogger
    {
        private readonly IPlatformLoggerImpl? _impl;
        private readonly LogLevel _minLevel;

        public PlatformLogger(IPlatformLoggerImpl? impl, LogLevel logLevel)
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

    public class PlatformLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public PlatformLoggerProvider(LogLevel logLevel)
        {
            IPlatformLoggerImpl? impl = DependencyService.Get<IPlatformLoggerImpl>();

            _logger = new PlatformLogger(impl, logLevel);
        }
        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        #region Dispose Pattern

        private bool _disposed;   // boolean flag to stop us calling Dispose(twice)

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool cleanManaged)
        {
            if (!_disposed)
            {
                if (cleanManaged)
                {
                    // managed

                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion
    }
}
