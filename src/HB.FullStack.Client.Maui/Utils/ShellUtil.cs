using Microsoft.Maui.Controls;

using System;

namespace HB.FullStack.Client.Maui.Utils
{
    public static class ShellUtil
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
