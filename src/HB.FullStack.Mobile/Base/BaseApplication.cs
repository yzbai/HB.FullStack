﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using HB.FullStack.Mobile.Api;
using HB.FullStack.Mobile.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Base
{
    public abstract class BaseApplication : Application
    {
        private LogLevel? _minimumLogLevel;
        private IConfiguration? _configuration;

        protected BaseApplication(IServiceCollection services)
        {
            //Version
            VersionTracking.Track();

            InitializeServices(services);
        }

        public IList<Task> InitializeTasks { get; } = new List<Task>();

        public Task InitializeTask { get => Task.WhenAll(InitializeTasks); }

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
                    if ("Debug".Equals(Environment, StringComparison.OrdinalIgnoreCase))
                    {
                        _minimumLogLevel = LogLevel.Information;
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

#pragma warning disable CA2109 // Review visible event handlers
        protected abstract void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e);
#pragma warning restore CA2109 // Review visible event handlers

        public abstract void PerformLogin();

        public abstract void DisplayOfflineWarning();


        private static IRemoteLoggingService? _remoteLoggingService;

        private static ILogger? _localLogger;

        public static void ExceptionHandler(Exception ex)
        {
            Log(LogLevel.Error, ex, null);


            if (ex is ApiException apiEx)
            {
                switch (apiEx.ErrorCode)
                {
                    case ErrorCode.ApiUnkown:

                        if (apiEx.HttpCode.IsNoInternet())
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                Application.Current.MainPage.DisplayAlert("网络异常", "看起来不能上网了，请联网吧!", "知道了").Fire();
                            });
                        }

                        break;
                    case ErrorCode.ApiNoAuthority:
                    case ErrorCode.ApiTokenRefresherError:
                    case ErrorCode.ApiTokenExpired:
                        Application.Current.PerformLogin();
                        break;
                    default: break;
                }
            }
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
