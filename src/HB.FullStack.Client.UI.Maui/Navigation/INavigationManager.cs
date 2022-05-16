using Microsoft.Maui.Controls;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace HB.FullStack.Client.UI.Maui
{
    public interface INavigationManager
    {
        Task GotoAsync(string uri, bool animated = false);

        Task GotoAsync(string uri, IDictionary<string, string> parameters, bool animated = false);

        Task GotoAsync(Page page, bool animated = false);

        Task GoBackAsync(bool animated = false);

        Task PushAsync(Page page, bool animated = false);

        Task PopAsync(bool animated = false);

        Task PushModalAsync(Page page, bool animated = false);

        Task PopModalAsync(bool animated = false);

    }
}