﻿using CommunityToolkit.Maui.Views;

using HB.FullStack.Client.Maui.Base;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityToolkit.Maui.Views
{ 
    public static class PopupExtensions
    {
        public static Task<object?> ShowAsync<TPopup>(this TPopup popup) where TPopup : Popup
        {
            return Current.Page.ShowPopupAsync(popup);
        }
    }
}
