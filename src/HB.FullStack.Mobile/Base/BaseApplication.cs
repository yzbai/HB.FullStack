using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Mobile.Api;
using HB.FullStack.Mobile.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace HB.FullStack.Mobile.Base
{
    public abstract class BaseApplication : Application
    {
        private ServiceProvider? _serviceProvider;
        private LogLevel? _minimumLogLevel;
        private IConfiguration? _configuration;
        private readonly IList<Task> _initializeTasks = new List<Task>();

        public Task InitializeTask { get => Task.WhenAll(_initializeTasks); }

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
                        _minimumLogLevel = LogLevel.Debug;
                    }
                    else
                    {
                        _minimumLogLevel = LogLevel.Information;
                    }
                }

                return _minimumLogLevel.Value;
            }
            set
            {
                _minimumLogLevel = value;
            }
        }

        public BaseApplication(IServiceCollection services)
        {
            //Version
            VersionTracking.Track();

            BaseRegisterServices(services);

            BaseConfigureServices();

            void BaseRegisterServices(IServiceCollection services)
            {
                services.AddOptions();

                services.AddLogging(builder =>
                {
                    builder.SetMinimumLevel(MinimumLogLevel);
                    builder.AddProvider(new LoggerProvider(MinimumLogLevel));
                });

                services.AddSingleton<TokenAutoRefreshedHttpClientHandler>();

                RegisterServices(services);

                //Build
                _serviceProvider = services.BuildServiceProvider();
                DependencyResolver.ResolveUsing(type => _serviceProvider.GetService(type));
            }

            void BaseConfigureServices()
            {
                //Log
                GlobalSettings.Logger = DependencyService.Resolve<ILogger<BaseApplication>>();
                GlobalSettings.MessageExceptionHandler = ExceptionHandler;
                //_remoteLoggingService = DependencyService.Resolve<IRemoteLoggingService>();

                //UriImageSourceEx
                UriImageSourceEx.HttpClientHandler = DependencyService.Resolve<TokenAutoRefreshedHttpClientHandler>();

                //Connectivity
                Connectivity.ConnectivityChanged += (s, e) => { OnConnectivityChanged(s, e); };

                ConfigureServices();
            }
        }

        public void AddInitTask(Task task)
        {
            task.Fire();
            _initializeTasks.Add(task);
        }

        public abstract string Environment { get; }

        protected abstract void RegisterServices(IServiceCollection services);

        protected abstract void ConfigureServices();

        protected abstract void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e);

        //public abstract void PerformLogin();

        public static void ExceptionHandler(Exception ex) => ExceptionHandler(ex, null);

        public static void ExceptionHandler(Exception? ex, string? message, LogLevel logLevel = LogLevel.Error)
        {
            if (ex is ApiException apiEx)
            {
                switch (apiEx.ErrorCode)
                {
                    case ApiErrorCode.ApiUnkown:

                        if (apiEx.HttpCode.IsNoInternet())
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                Application.Current.MainPage.DisplayAlert("网络异常", "看起来不能上网了，请联网吧!", "知道了").Fire();
                            });
                        }

                        break;
                    case ApiErrorCode.ApiNotAvailable:
                    case ApiErrorCode.NoAuthority:
                    case ApiErrorCode.AccessTokenExpired:
                        //Application.Current.PerformLogin();
                        break;
                    default: break;
                }
            }

            Log(ex, message, logLevel);
        }

        public abstract void OnOfflineDataUsed();

        public static void Log(Exception? ex, string? message = null, LogLevel logLevel = LogLevel.Error)
        {
            //_remoteLoggingService.LogAsync(logLevel, ex, message); //这里不加Fire()。避免循环

            GlobalSettings.Logger.Log(logLevel, ex, message);
        }
    }
}
