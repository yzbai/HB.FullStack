using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HB.Framework.Client.iOS;
using HB.Framework.Client.UI.Platform;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformStatusBar))]
namespace HB.Framework.Client.iOS
{
    public class PlatformStatusBar : IStatusBar
    {
        public void Show()
        {
            UIApplication.SharedApplication.StatusBarHidden = false;
        }

        public void Hide()
        {
            UIApplication.SharedApplication.StatusBarHidden = true;
        }
    }
}
