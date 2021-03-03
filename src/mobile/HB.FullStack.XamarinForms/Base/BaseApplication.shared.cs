using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.XamarinForms.Api;
using HB.FullStack.XamarinForms.Controls;
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

        protected BaseApplication()
        {
            //Version
            VersionTracking.Track();

            NavigationService.Init(GetNavigationServiceImpl());
        }

        protected abstract NavigationService GetNavigationServiceImpl();

        protected void InitializeServices(IServiceCollection services)
        {
            RegisterBaseServices(services);

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

        protected abstract void RegisterServices(IServiceCollection services);

        protected abstract void ConfigureServices();

        public static void ExceptionHandler(Exception ex) => ExceptionHandler(ex, null);

        public static void ExceptionHandler(Exception? ex, string? message, LogLevel logLevel = LogLevel.Error)
        {
            if (ex is ApiException apiEx)
            {
                switch (apiEx.ErrorCode)
                {
                    case ApiErrorCode.NoAuthority:
                        break;
                    case ApiErrorCode.AccessTokenExpired:
                        break;
                    case ApiErrorCode.ModelValidationError:
                        break;
                    case ApiErrorCode.ApiNotAvailable:
                        break;
                    case ApiErrorCode.ApiErrorUnkownFormat:
                        break;
                    case ApiErrorCode.NotApiResourceEntity:
                        break;
                    case ApiErrorCode.ApiSmsCodeInvalid:
                        //TODO: 使用Toast随后,
                        //TODO: 不显示，调查
                        Application.Current.MainPage.DisplayAlert("","短信验证码不对", "知道了");
                        break;
                    case ApiErrorCode.ApiPublicResourceTokenNeeded:
                        break;
                    case ApiErrorCode.ApiPublicResourceTokenError:
                        break;
                    case ApiErrorCode.ApiUploadEmptyFile:
                        break;
                    case ApiErrorCode.ApiUploadOverSize:
                        break;
                    case ApiErrorCode.ApiUploadWrongType:
                        break;
                    case ApiErrorCode.ApiHttpsRequired:
                        break;
                    case ApiErrorCode.FromExceptionController:
                        break;
                    case ApiErrorCode.ApiCapthaError:
                        break;
                    case ApiErrorCode.ApiUploadFailed:
                        break;
                    case ApiErrorCode.ServerError:
                        break;
                    case ApiErrorCode.ClientError:
                        break;
                    default:
                        break;
                }
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
        public static void LogDebug(string message)
        {
            Log(null, message, LogLevel.Debug);
        }

        public static void LogError(string message)
        {
            Log(null, message, LogLevel.Error);
        }
    }
}
