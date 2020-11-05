using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Xamarin.Forms
{
    public static class ShellExtensions
    {
        public static bool IsCurrentPageA(this Shell shell, Type pageType)
        {
            return shell.CurrentPage().GetType() == pageType;
        }

        public static Page CurrentPage(this Shell shell)
        {
            return ((IShellSectionController)shell.CurrentItem.CurrentItem).PresentedPage;
        }

        public static string? GetPageName(this ShellNavigationState? state)
        {
            if (state == null)
                return null;

            string? routing = state.Location.OriginalString;

            if (routing == null)
            {
                return null;
            }

            string[]? segments = routing.Split('/');

            if (segments.IsNullOrEmpty())
            {
                return null;
            }

            return segments[^1];
        }
    }
}
