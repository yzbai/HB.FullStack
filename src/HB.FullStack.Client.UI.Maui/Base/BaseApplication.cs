using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Client.File;
using HB.FullStack.Client.Network;

using Microsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace HB.FullStack.Client.UI.Maui
{

    public class InitializeService : IMauiInitializeService
    {
        public void Initialize(IServiceProvider services)
        {

        }
    }

    public class InitalizeScopedService : IMauiInitializeScopedService
    {
        public void Initialize(IServiceProvider services)
        {
            //查看application.current等等是否都已经准备好

            INavigationManager.Current = services.GetRequiredService<INavigationManager>();
        }
    }

    public abstract class BaseApplication : Application//置各种Current
    {
        public new static BaseApplication? Current => (BaseApplication?)Application.Current;


        public static string Environment =>
#if DEBUG
            "Debug";
#endif
#if RELEASE
            "Release";
#endif
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

                services.AddKVManager();

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

            using Stream? resFileStream = executingAssembly.GetManifestResourceStream(fileName);

            IConfigurationBuilder builder = new ConfigurationBuilder();

            builder.AddJsonStream(resFileStream);

            return builder.Build();
        }
    }
}