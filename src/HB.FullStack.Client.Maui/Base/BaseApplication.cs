using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

using System;

namespace HB.FullStack.Client.Maui.Base
{
    public abstract class BaseApplication : Application
    {
        public new static BaseApplication Current => (BaseApplication?)Application.Current!;

        public static Page CurrentPage
        {
            get
            {
                if (Shell.Current != null)
                {
                    return Shell.Current.CurrentPage;
                }

                if (Application.Current?.MainPage?.Navigation.ModalStack.Count > 0)
                {
                    return Application.Current.MainPage.Navigation.ModalStack[0];
                }

                if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 0)
                {
                    return Application.Current.MainPage.Navigation.NavigationStack[0];
                }

                return Application.Current!.MainPage!;
            }
        }

        public static IDispatcher CurrentDispatcher => CurrentPage.Dispatcher;

        #region Lifecycle

        protected override void OnResume()
        {
            base.OnResume();

            SubscribeMessages();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            UnSubscribeMessages();
        }

        #endregion

        #region Messaging

        private void SubscribeMessages()
        {
            MessagingCenter.Subscribe<BaseViewModel, ExceptionDisplayArguments>(this, BaseViewModel.ExceptionDisplaySignalName, OnExceptionDisplayRequested);
        }

        private void UnSubscribeMessages()
        {
            MessagingCenter.Unsubscribe<BaseViewModel, ExceptionDisplayArguments>(this, BaseViewModel.ExceptionDisplaySignalName);
        }

        private async void OnExceptionDisplayRequested(BaseViewModel viewModel, ExceptionDisplayArguments arg)
        {
            await CurrentPage.DisplayAlert("Exception", arg.Message, "OK").ConfigureAwait(false);
        }

        #endregion
    }
}