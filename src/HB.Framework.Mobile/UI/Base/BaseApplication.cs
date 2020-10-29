using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AsyncAwaitBestPractices;
using HB.Framework.Client.Api;
using HB.Framework.Client.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.Framework.Client.Base
{
    public abstract class BaseApplication : Application
    {
        private LogLevel? _minimumLogLevel;
        private IConfiguration? _configuration;

        public BaseApplication(IServiceCollection services)
        {
            //Version
            VersionTracking.Track();

            InitializeServices(services);
        }

        public IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = ClientUtils.BuildConfiguration($"appsettings.{Environment}.json", Assembly.GetCallingAssembly());
                }

                return _configuration;
            }
        }

        public LogLevel MinimumLogLevel
        {
            get
            {
                if (_minimumLogLevel == null)
                {
                    if ("Debug".Equals(Environment, StringComparison.InvariantCultureIgnoreCase))
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

        public abstract string Environment { get; }

        private void InitializeServices(IServiceCollection services)
        {
            RegisterServices(services);

            ConfigureServices();
        }

        protected virtual void RegisterServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(MinimumLogLevel);
                builder.AddProvider(new PlatformLoggerProvider(LogLevel.Trace));
            });

            services.AddSingleton<FFImageLoadingAutoRefreshJwtHttpClientHandler>();
        }

        protected virtual void ConfigureServices()
        {
            //Connectivity
            Connectivity.ConnectivityChanged += (s, e) => { OnConnectivityChanged(s, e); };
        }

        protected virtual void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {

        }



        private static IRemoteLoggingService? _remoteLoggingService;
        private static ILogger? _localLogger;

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
    }
}
