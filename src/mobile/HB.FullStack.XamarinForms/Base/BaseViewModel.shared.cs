﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Base
{
    public abstract class BaseViewModel : ObservableObject
    {

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public virtual void OnAppearing(string pageName)
        {
        }

        public virtual void OnDisappearing(string pageName)
        {

        }

        public static void DisplayAlert(string message, string title, string button)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage.DisplayAlert(message, title, button).Fire();
            });
        }

        public static Task<bool> DisplayChoiceAlertAsync(string message, string title, string acceptButton, string cancelButton)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, acceptButton, cancelButton);
        }

        protected static void PopToRoot()
        {
            Device.BeginInvokeOnMainThread(() => BaseApplication.Current.Navigation?.PopToRootAsync().Fire());
        }

        protected static void Pop()
        {
            Device.BeginInvokeOnMainThread(() => BaseApplication.Current.Navigation?.PopAsync().Fire());
        }

        protected static void Push(Page page, bool animated = true)
        {
            Device.BeginInvokeOnMainThread(()=> BaseApplication.Current.Navigation?.PushAsync(page, animated).Fire());
        }

        protected static void PushModal(BaseModalDialog dialog, bool animate = true)
        {
            Device.BeginInvokeOnMainThread(() => BaseApplication.Current.Navigation?.PushModalAsync(dialog, animate).Fire());
        }

        protected static void GotoMainPage()
        {
            Device.BeginInvokeOnMainThread(() => BaseApplication.Current.GotoMainPage());
        }
    }
}
