using AsyncAwaitBestPractices;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using HB.FullStack.Client.Maui.Base;
using HB.FullStack.Client.Maui.Controls.Popups;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Useful Shortcuts
    /// </summary>
    public static class Currents
    {
        private static IConfiguration? _configuration;
        private static PopupSizeConstants? _popupSizeConstants;

        #region Environment

        public static string Environment =>
#if DEBUG
    "Debug";
#endif
#if RELEASE
     "Release";
#endif

        public static bool IsDebug => Environment == "Debug";

        /// <summary>
        /// 要确保在App项目中调用
        /// </summary>
        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration != null)
                {
                    return _configuration;
                }

                string appsettingsFile = $"appsettings.{Environment}.json";
                Assembly assembly = Assembly.GetCallingAssembly();

                string fileName = $"{assembly.FullName!.Split(',')[0]}.{appsettingsFile}";

                using Stream? resFileStream = assembly.GetManifestResourceStream(fileName);

                IConfigurationBuilder builder = new ConfigurationBuilder();

                builder.AddJsonStream(resFileStream);

                _configuration = builder.Build();

                return _configuration;
            }
        }

        public static IServiceProvider Services => IPlatformApplication.Current!.Services;

        #endregion

        #region Controls

        public static BaseApplication Application => (BaseApplication?)Microsoft.Maui.Controls.Application.Current!;

        public static Window? Window => Application.MainPage?.Window;

        public static Page Page
        {
            get
            {
                if (Shell.Current != null)
                {
                    return Shell.Current.CurrentPage ?? Shell.Current;
                }

                if (Microsoft.Maui.Controls.Application.Current?.MainPage?.Navigation.ModalStack.Count > 0)
                {
                    return Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.ModalStack[0];
                }

                if (Microsoft.Maui.Controls.Application.Current?.MainPage?.Navigation.NavigationStack.Count > 0)
                {
                    return Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.NavigationStack[0];
                }

                return Microsoft.Maui.Controls.Application.Current!.MainPage!;
            }
        }

        public static Shell Shell => Shell.Current;

        public static INavigation Navigation => Shell.Current.Navigation;

        public static IDispatcher Dispatcher => Page.Dispatcher;

        public static PopupSizeConstants PopupSizeConstants => _popupSizeConstants ??= Services.GetRequiredService<PopupSizeConstants>();

        #endregion

        #region Pops

        public static void ShowToast(string message, ToastDuration duration = ToastDuration.Long, double textSize = 14)
        {
            //TODO: 错误上报
            Toast
                .Make(message, duration, textSize)
                .Show()
                .SafeFireAndForget(ex => { GlobalSettings.Logger.LogCritical(ex, "ViewModel的OnException显示Alert挂了."); });
        }

        #endregion

        public static IList<Task> AppendingTasks { get; } = new List<Task>();
    }

    public static class CurrentsExtensions
    {
        public static Task GoToAsync(this Shell shell, string uri, IDictionary<string, object?> parameters)
        {
            return shell.GoToAsync(BuildUri(uri, parameters));
        }

        public static Task GoBackAsync(this Shell shell, IDictionary<string, object?>? parameters) => shell.GoToAsync("..", parameters);

        public static Task GoBackAsync(this Shell shell) => shell.GoToAsync("..");

        private static string BuildUri(string uri, IDictionary<string, object?>? parameters)
        {
            if (parameters.IsNullOrEmpty())
            {
                return uri;
            }

            StringBuilder fullUrl = new StringBuilder();
            fullUrl.Append(uri);
            fullUrl.Append('?');
            fullUrl.Append(string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            return fullUrl.ToString();
        }
    }

}