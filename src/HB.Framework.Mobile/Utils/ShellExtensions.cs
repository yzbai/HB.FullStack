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
            if (shell?.CurrentItem?.CurrentItem is IShellSectionController shellSectionController)
            {
                return shellSectionController.PresentedPage.GetType() == pageType;
            }

            return false;
        }
    }
}
