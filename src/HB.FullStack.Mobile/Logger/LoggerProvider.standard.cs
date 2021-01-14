using HB.FullStack.Mobile.Platforms;
using Microsoft.Extensions.Logging;
using System;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Logger
{
    public class LoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public LoggerProvider(LogLevel logLevel)
        {
            IPlatformLoggerImpl? impl = DependencyService.Get<IPlatformLoggerImpl>();

            _logger = new Logger(impl, logLevel);
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
