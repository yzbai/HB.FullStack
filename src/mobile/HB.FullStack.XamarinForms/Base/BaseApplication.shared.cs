using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Client;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.XamarinForms.Logging;
using HB.FullStack.XamarinForms.Navigation;

using Microsoft;
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

        public new static BaseApplication Current => (BaseApplication)Application.Current;

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
                    _configuration = GetConfiguration($"appsettings.{Environment}.json", Assembly.GetCallingAssembly());
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

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                //TODO: 设置这个

                ExceptionHandler(e.Exception);
                e.SetObserved();
            };

            //Version
            VersionTracking.Track();
        }

        protected void InitializeServices(IServiceCollection services)
        {
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

                services.AddSingleton<IPreferenceProvider, XFPreferenceProvider>();
                services.AddSingleton<ConnectivityManager, XFConnectivityManager>();
                services.AddSingleton<NavigationManager, XFNavigationManager>();
                services.AddSingleton<ImageSourceManager>();

                services.AddKV();

                RegisterServices(services);

                //Build
                _serviceProvider = services.BuildServiceProvider();
                DependencyResolver.ResolveUsing(type => _serviceProvider.GetService(type));
            }

            void ConfigureBaseServices()
            {
                //FileService
                if (VersionTracking.IsFirstLaunchEver)
                {
                    IFileManager fileManager = DependencyService.Resolve<IFileManager>();
                    AddInitTask(fileManager.UnzipAssetZipAsync(InitAssetFileName));
                }

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

        protected abstract void RegisterServices(IServiceCollection services);

        protected abstract void ConfigureServices();

        public static void ExceptionHandler(Exception ex) => ExceptionHandler(ex, null);

        public static void ExceptionHandler(Exception? ex, string? message, LogLevel logLevel = LogLevel.Error)
        {
            //TODO: ExceptionHandler
            if (ex is ApiException _)
            {
            }
            else if (ex is ClientException _)
            {
            }
            else if (ex is DatabaseException _)
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

        public static IConfiguration GetConfiguration(string appsettingsFile, [ValidatedNotNull] Assembly executingAssembly)
        {
            ThrowIf.Empty(appsettingsFile, nameof(appsettingsFile));

            string fileName = $"{executingAssembly.FullName!.Split(',')[0]}.{appsettingsFile}";

            using Stream resFileStream = executingAssembly.GetManifestResourceStream(fileName);

            IConfigurationBuilder builder = new ConfigurationBuilder();

            builder.AddJsonStream(resFileStream);

            return builder.Build();
        }
    }
}