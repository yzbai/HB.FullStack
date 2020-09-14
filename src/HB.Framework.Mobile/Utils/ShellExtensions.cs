using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Xamarin.Forms
{
    public static class ShellExtensions
    {
        public static bool CurrentPageIsA(this Shell shell, Type pageType)
        {
            return shell.CurrentPage().GetType() == pageType;
        }

        public static Page CurrentPage(this Shell shell)
        {
            return ((IShellSectionController)shell.CurrentItem.CurrentItem).PresentedPage;
        }
    }
}
