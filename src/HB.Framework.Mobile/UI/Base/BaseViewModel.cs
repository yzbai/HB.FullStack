using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace HB.Framework.Client.Base
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

        public IEnumerable<ValidationResult> ValidationResults { get { return GetValidateResults(); } }

        public BaseViewModel()
        {

        }

        public void NotifyValidationChanged(string? proerptyName = null)
        {
            PerformValidate(proerptyName);
            OnPropertyChanged(nameof(ValidationResults));
        }

        public virtual void OnAppearing()
        {
            NotifyValidationChanged();
        }

        public virtual void OnDisappearing()
        {

        }

        public static void DisplayAlert(string message, string title, string button)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage.DisplayAlert(message, title, button).Fire();
            });
        }
    }
}
