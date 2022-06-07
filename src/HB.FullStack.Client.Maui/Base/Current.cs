using AsyncAwaitBestPractices;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using HB.FullStack.Client.Maui.Base;
using HB.FullStack.Client.Maui.Controls.Popups;
using HB.FullStack.Client.Navigation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

using System;

namespace System
{
    public static class Currents
    {
        public static BaseApplication Application => (BaseApplication?)Microsoft.Maui.Controls.Application.Current!;

        public static IServiceProvider Services => IPlatformApplication.Current!.Services;

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

        public static IDispatcher Dispatcher => Page.Dispatcher;

        public static INavigationManager NavigationManager => INavigationManager.Current;

        private static PopupSizeConstants? _popupSizeConstants;
        public static PopupSizeConstants PopupSizeConstants => _popupSizeConstants ??= Services.GetRequiredService<PopupSizeConstants>();


        #region UIs

        public static void ShowToast(string message, ToastDuration duration = ToastDuration.Long, double textSize = 14)
        {
            //TODO: 错误上报
            Toast
                .Make(message, duration, textSize)
                .Show()
                .SafeFireAndForget(ex => { GlobalSettings.Logger.LogCritical(ex, "ViewModel的OnException显示Alert挂了."); });
        }

        #endregion
    }


}