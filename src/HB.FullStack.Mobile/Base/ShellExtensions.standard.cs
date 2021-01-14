using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Xamarin.Forms
{
    public static class ShellHelper
    {
        public static bool IsCurrentPageA(Type pageType)
        {
            return Shell.Current.CurrentPage.GetType() == pageType;
        }

        public static string GetCurrentPageName()
        {
            return Shell.Current.CurrentPage.GetType().Name;
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
