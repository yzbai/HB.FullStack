using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using HB.FullStack.Client.Navigation;

namespace HB.FullStack.Client.Maui
{
    public class MauiNavigationManager : INavigationManager
    {
        public async Task GoBackAsync(IDictionary<string, object?>? parameters = null)
        {
            // TODO: Work out why GoToAsync("..") throws an exception with modal pages
            while (Shell.Current.Navigation.ModalStack.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync();
            }

            await GotoAsync("..", parameters);
        }

        public async Task GotoAsync(string uri, IDictionary<string, object?>? parameters = null)
        {
            string fullUri = BuildUri(uri, parameters);
            await Shell.Current.GoToAsync(fullUri, Animated);
        }

        public async Task PopAsync()
        {
            await Shell.Current.Navigation.PopAsync(Animated);
        }

        public async Task PopModalAsync()
        {
            await Shell.Current.Navigation.PopModalAsync(Animated);
        }

        public async Task PushAsync(string pageFullName)
        {
            Page? page = Currents.Services.GetPage(pageFullName);

            if (page == null)
            {
                return;
            }

            await Shell.Current.Navigation.PushAsync(page, Animated);
        }

        public async Task PushModalAsync(string pageFullName)
        {
            Page? page = Currents.Services.GetPage(pageFullName);

            if (page == null)
            {
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(page, Animated);
        }

        public Task PushAsync(IBaseViewModel baseViewModel)
        {
            throw new NotImplementedException();
        }

        public Task PushModalAsync(IBaseViewModel baseViewModel)
        {
            throw new NotImplementedException();
        }

        private static string BuildUri(string uri, IDictionary<string, object?>? parameters)
        {
            if(parameters.IsNullOrEmpty())
            {
                return uri;
            }

            StringBuilder fullUrl = new StringBuilder();
            fullUrl.Append(uri);
            fullUrl.Append('?');
            fullUrl.Append(string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            return fullUrl.ToString();
        }

        public async Task PushAsync(object page)
        {
            if (page is not Page item)
            {
                return;
            }

            await Shell.Current.Navigation.PushAsync(item, Animated);
        }

        public async Task PushModalAsync(object page)
        {
            if (page is not Page item)
            {
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(item, Animated);
        }

        public bool Animated { get; set; }
    }
}