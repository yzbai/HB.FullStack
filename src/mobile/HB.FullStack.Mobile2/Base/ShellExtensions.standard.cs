using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
