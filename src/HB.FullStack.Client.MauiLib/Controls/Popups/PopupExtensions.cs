using CommunityToolkit.Maui.Views;
using HB.FullStack.Client.Maui;
using HB.FullStack.Client.Maui.Base;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public static class PopupExtensions
    {
        public static Task<object?> ShowAsync<TPopup>(this TPopup popup) where TPopup : Popup
        {
            return Currents.Page.ShowPopupAsync(popup);
        }
    }
}
