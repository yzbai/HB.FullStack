using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace HB.FullStack.Client.Base
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

        public void NotifyValidationChanged([CallerMemberName] string? proerptyName = null)
        {
            PerformValidate(proerptyName);
            OnPropertyChanged(nameof(ValidationResults));
        }

        public virtual void OnAppearing(string pageName)
        {
            NotifyValidationChanged();
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

        public static Task<bool> DisplayAlertAsync(string message, string title, string acceptButton, string cancelButton)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, acceptButton, cancelButton);
        }
    }
}
