using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AsyncAwaitBestPractices;
using HB.Framework.Client.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xamarin.Forms;

namespace HB.Framework.Client.Base
{
    public abstract class BaseApplication : Application
    {
        public BaseApplication(IServiceCollection services)
        {
            InitializeServices(services);
        }

        private void InitializeServices(IServiceCollection services)
        {
            ConfigureServices(services);

            InitializeServices();
        }

        protected abstract void ConfigureServices(IServiceCollection services);

        protected abstract void InitializeServices();

        private static IConfiguration? _configuration;

        public IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = ClientUtils.BuildConfiguration($"appsettings.{GetEnvironment()}.json", Assembly.GetCallingAssembly());
                }

                return _configuration;
            }
        }

        public abstract string GetEnvironment();

        public static void ExceptionHandler(Exception ex)
        {
            Log(LogLevel.Error, ex, null);
        }

        public static void Log(LogLevel logLevel, Exception? ex, string? message = null)
        {
            if (_remoteLoggingService == null)
            {
                _remoteLoggingService = DependencyService.Resolve<IRemoteLoggingService>();
            }

            _remoteLoggingService?.LogAsync(logLevel, ex, message).SafeFireAndForget();

            if (_localLogger == null)
            {
                _localLogger = DependencyService.Resolve<ILogger<BaseApplication>>();
            }

            _localLogger?.Log(logLevel, ex, message);
        }

        private static IRemoteLoggingService? _remoteLoggingService;
        private static ILogger? _localLogger;

        private LogLevel? _minimumLogLevel;

        public LogLevel MinimumLogLevel
        {
            get
            {
                if (_minimumLogLevel == null)
                {
                    string? environment = GetEnvironment();

                    if ("Debug".Equals(environment, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _minimumLogLevel = LogLevel.Trace;
                    }
                    else
                    {
                        _minimumLogLevel = LogLevel.Information;
                    }
                }

                return _minimumLogLevel.Value;
            }
        }
    }
}
