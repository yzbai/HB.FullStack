using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HB.FullStack.Mobile.iOS;
using HB.FullStack.Mobile.Platforms;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformStatusBar))]
namespace HB.FullStack.Mobile.iOS
{
    public class PlatformStatusBar : IPlatformStatusBarHelper
    {
        public void Show()
        {
            UIApplication.SharedApplication.StatusBarHidden = false;
        }

        public void Hide()
        {
            UIApplication.SharedApplication.StatusBarHidden = true;
        }

        public bool IsShowing { get => UIApplication.SharedApplication.StatusBarHidden; }
    }
}
