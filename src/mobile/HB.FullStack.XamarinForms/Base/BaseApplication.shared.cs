using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.XamarinForms.Api;
using HB.FullStack.XamarinForms.Controls;
using HB.FullStack.XamarinForms.Files;
using HB.FullStack.XamarinForms.Logging;
using HB.FullStack.XamarinForms.Platforms;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace HB.FullStack.XamarinForms.Base
{
    public abstract class BaseApplication : Application
    {
        private ServiceProvider? _serviceProvider;
        private LogLevel? _minimumLogLevel;
        private IConfiguration? _configuration;
        private readonly IList<Task> _initializeTasks = new List<Task>();

        public static new BaseApplication Current => (BaseApplication)Application.Current;

        public static IPlatformHelper PlatformHelper = DependencyService.Resolve<IPlatformHelper>();

#if DEBUG
        public static string Environment => "Debug";
#endif
#if RELEASE
        public static string Environment => "Release";
#endif

        public Task InitializeTask { get => Task.WhenAll(_initializeTasks); }

        public IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = ClientUtils.GetConfiguration($"appsettings.{Environment}.json", Assembly.GetCallingAssembly());
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

        public abstract string InitAssetFileName { get; set; }

        protected BaseApplication()
        {
            //Version
            VersionTracking.Track();

            //FileService
            if (VersionTracking.IsFirstLaunchEver)
            {
                AddInitTask(IFileService.UnzipInitFilesAsync(InitAssetFileName));
            }
        }

        protected void InitializeServices(IServiceCollection services)
        {
            //设置导航
            NavigationService.Init(GetNavigationServiceImpl());

            //注册服务
            RegisterBaseServices(services);

            //配置服务
            ConfigureBaseServices();

            void RegisterBaseServices(IServiceCollection services)
            {
                services.AddOptions();

                services.AddLogging(builder =>
                {
                    builder.SetMinimumLevel(MinimumLogLevel);
                    builder.AddProvider(new LoggerProvider(MinimumLogLevel));
                });

                RegisterServices(services);

                //Build
                _serviceProvider = services.BuildServiceProvider();
                DependencyResolver.ResolveUsing(type => _serviceProvider.GetService(type));
            }

            void ConfigureBaseServices()
            {
                //Log
                GlobalSettings.Logger = DependencyService.Resolve<ILogger<BaseApplication>>();
                GlobalSettings.MessageExceptionHandler = ExceptionHandler;
                //_remoteLoggingService = DependencyService.Resolve<IRemoteLoggingService>();

                //Connectivity
                //Connectivity.ConnectivityChanged += (s, e) => { OnConnectivityChanged(s, e); };

                ConfigureServices();
            }
        }

        public void AddInitTask(Task task)
        {
            task.Fire();
            _initializeTasks.Add(task);
        }

        protected abstract NavigationService GetNavigationServiceImpl();

        protected abstract void RegisterServices(IServiceCollection services);

        protected abstract void ConfigureServices();

        public static void ExceptionHandler(Exception ex) => ExceptionHandler(ex, null);

        public static void ExceptionHandler(Exception? ex, string? message, LogLevel logLevel = LogLevel.Error)
        {
            if (ex is ApiException apiEx)
            {
            }
            else if (ex is MobileException mobileException)
            {

            }
            else if (ex is DatabaseException dbException)
            {

            }

            Log(ex, message, logLevel);
        }


        public abstract void OnOfflineDataUsed();

        public static void Log(Exception? ex, string? message = null, LogLevel logLevel = LogLevel.Error)
        {
            //_remoteLoggingService.LogAsync(logLevel, ex, message); //这里不加Fire()。避免循环

            GlobalSettings.Logger.Log(logLevel, ex, message);
        }
        public static void LogDebug(string message, Exception? ex = null)
        {
            Log(ex, message, LogLevel.Debug);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            Log(ex, message, LogLevel.Error);
        }
    }
}
