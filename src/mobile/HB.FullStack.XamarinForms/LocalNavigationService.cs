using HB.FullStack.XamarinForms.Base;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms
{
    public class LocalNavigationService : NavigationService
    {
        public override async Task GoBackAsync(bool animated = false)
        {
            // TODO: Work out why GoToAsync("..") throws an exception with modal pages
            while (Shell.Current.Navigation.ModalStack.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync().ConfigureAwait(false);
            }

            await Shell.Current.GoToAsync("..").ConfigureAwait(false);
        }

        public override async Task GotoAsync(string uri, bool animated = false)
        {
            await Shell.Current.GoToAsync(uri, animated).ConfigureAwait(false);
        }

        public override async Task GotoAsync(string uri, IDictionary<string, string> parameters, bool animated =false)
        {
            string fullUri = BuildUri(uri, parameters);
            await Shell.Current.GoToAsync(fullUri, animated).ConfigureAwait(false);
        }

        public override async Task GotoAsync(Page page, bool animated = false)
        {
            await Shell.Current.Navigation.PushAsync(page, animated).ConfigureAwait(false);
        }

        public override async Task PopAsync(bool animated = false)
        {
            await Shell.Current.Navigation.PopAsync(animated).ConfigureAwait(false);
        }

        public override async Task PopModalAsync(bool animated = false)
        {
            await Shell.Current.Navigation.PopModalAsync(animated).ConfigureAwait(false);
        }

        public override async Task PushAsync(Page page, bool animated = false)
        {
            await Shell.Current.Navigation.PushAsync(page, animated).ConfigureAwait(false);
        }

        public override async Task PushModalAsync(Page page, bool animated = false)
        {
            await Shell.Current.Navigation.PushModalAsync(page, animated).ConfigureAwait(false);
        }

        private static string BuildUri(string uri, IDictionary<string, string> parameters)
        {
            var fullUrl = new StringBuilder();
            fullUrl.Append(uri);
            fullUrl.Append('?');
            fullUrl.Append(string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            return fullUrl.ToString();
        }
    }
}