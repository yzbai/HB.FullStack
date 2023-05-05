/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using AsyncAwaitBestPractices;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Client.MauiLib.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Storage;

using System;
using System.IO;
using System.Reflection;

namespace HB.FullStack.Client.MauiLib
{
    /// <summary>
    /// Useful Shortcuts, mostly UIs
    /// </summary>
    public static class Currents
    {
        private const string Development = nameof(Development);
        private const string Production = nameof(Production);

        //private static IConfiguration? _configuration;
        private static PopupSizeConstants? _popupSizeConstants;

        #region Environment

        public static string Environment =>
#if DEBUG
    Development;

#endif
#if RELEASE
    Production;
#endif

        public static bool IsDevelopment => Environment == Development;

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

        public static string PageName => Page.GetType().Name;

        public static Shell Shell => Shell.Current;

        public static INavigation Navigation => Shell.Current.Navigation;

        //https://github.com/dotnet/maui/discussions/7518
        //public static IDispatcher Dispatcher => Page.Dispatcher;

        public static PopupSizeConstants PopupSizeConstants => _popupSizeConstants ??= Services.GetRequiredService<PopupSizeConstants>();

        public static string AppDataDirectory => FileSystem.AppDataDirectory;

        //public static string DbFileDirectory => Path.Combine(FileSystem.AppDataDirectory, "dbs");

        public static string CacheDirectory => FileSystem.CacheDirectory;

        #endregion

        #region Toast

        public static void ShowToast(string message, ToastDuration duration = ToastDuration.Long, double textSize = 14)
        {
            //TODO: 错误上报
            Toast
                .Make(message, duration, textSize)
                .Show()
                .SafeFireAndForget(ex => { Globals.Logger.LogCritical(ex, "ViewModel的OnException显示Alert挂了."); });
        }

        #endregion

        public static bool IsIntroducedYet { get => ClientPreferences.IsIntroducedYet; set => ClientPreferences.IsIntroducedYet = value; }

        //public static IList<Task> AppendingTasks { get; } = new List<Task>();
    }
}