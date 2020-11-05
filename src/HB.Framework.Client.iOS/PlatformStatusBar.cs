using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HB.Framework.Client.iOS;
using HB.Framework.Client.Platforms;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformStatusBar))]
namespace HB.Framework.Client.iOS
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
