using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace HB.FullStack.Client.Maui
{
    public class MauiNavigationManager : INavigationManager
    {
        public async Task GoBackAsync(bool animated = false)
        {
            // TODO: Work out why GoToAsync("..") throws an exception with modal pages
            while (Shell.Current.Navigation.ModalStack.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync().ConfigureAwait(false);
            }

            await Shell.Current.GoToAsync("..").ConfigureAwait(false);
        }

        public async Task GotoAsync(string uri, bool animated = false)
        {
            await Shell.Current.GoToAsync(uri, animated).ConfigureAwait(false);
        }

        public async Task GotoAsync(string uri, IDictionary<string, string> parameters, bool animated = false)
        {
            string fullUri = BuildUri(uri, parameters);
            await Shell.Current.GoToAsync(fullUri, animated).ConfigureAwait(false);
        }

        public async Task PopAsync(bool animated = false)
        {
            await Shell.Current.Navigation.PopAsync(animated).ConfigureAwait(false);
        }

        public async Task PopModalAsync(bool animated = false)
        {
            await Shell.Current.Navigation.PopModalAsync(animated).ConfigureAwait(false);
        }

        public async Task PushAsync(string pageFullName, bool animated = false)
        {
            Page? page = ServicesProviderUtil.GetPage(pageFullName);

            if (page == null)
            {
                return;
            }

            await Shell.Current.Navigation.PushAsync(page, animated).ConfigureAwait(false);
        }

        public async Task PushModalAsync(string pageFullName, bool animated = false)
        {
            Page? page = ServicesProviderUtil.GetPage(pageFullName);

            if(page == null)
            {
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
        }

        public Task PushAsync(IBaseViewModel baseViewModel, bool animated = false)
        {
            throw new NotImplementedException();
        }

        public Task PushModalAsync(IBaseViewModel baseViewModel, bool animated = false)
        {
            throw new NotImplementedException();
        }

        private static string BuildUri(string uri, IDictionary<string, string> parameters)
        {
            var fullUrl = new StringBuilder();
            fullUrl.Append(uri);
            fullUrl.Append('?');
            fullUrl.Append(string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            return fullUrl.ToString();
        }

        public async Task PushAsync(object page, bool animated = false)
        {
            if (page is not Page item)
            {
                return;
            }

            await Shell.Current.Navigation.PushAsync(item, animated).ConfigureAwait(false);
        }

        public async Task PushModalAsync(object page, bool animated = false)
        {
            if (page is not Page item)
            {
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(item, animated).ConfigureAwait(false);
        }
    }
}