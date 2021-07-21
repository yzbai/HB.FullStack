using System;
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
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// page将要呈现时执行，
        /// page和viewmodel的生命周期不一定一致。
        /// </summary>
        /// <param name="pageTypeName"></param>
        /// <returns></returns>
        public virtual Task OnAppearingAsync(string pageTypeName) 
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// page将要消失时执行，
        /// page和viewmodel的生命周期不一定一致。
        /// </summary>
        /// <param name="pageTypeName"></param>
        /// <returns></returns>
        public virtual Task OnDisappearingAsync(string pageTypeName)
        {
            return Task.CompletedTask;
        }

        public static void DisplayAlert(string message, string title, string button)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(message, title, button).ConfigureAwait(false);
            });
        }

        public static Task<bool> DisplayChoiceAlertAsync(string message, string title, string acceptButton, string cancelButton)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, acceptButton, cancelButton);
        }

    }
}
