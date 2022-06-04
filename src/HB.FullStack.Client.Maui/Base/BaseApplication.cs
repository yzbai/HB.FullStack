using CommunityToolkit.Maui.Views;

using HB.FullStack.Client.Navigation;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Base
{
    public abstract class BaseApplication : Application
    {
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
            await Currents.Page.DisplayAlert("Exception", arg.Message, "OK").ConfigureAwait(false);
        }

        #endregion
    }
}