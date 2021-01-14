using Android.Views;
using HB.FullStack.Mobile.Droid.Platforms;
using HB.FullStack.Mobile.Platforms;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(StatusBarHelper))]
namespace HB.FullStack.Mobile.Droid.Platforms
{
    public class StatusBarHelper : IPlatformStatusBarHelper
    {
        private WindowManagerFlags _orginalFlags;

        public bool IsShowing { get; set; } = true;

        public void Show()
        {
            if (IsShowing)
            {
                return;
            }

            var attrs = Platform.CurrentActivity.Window.Attributes;

            attrs.Flags = _orginalFlags;

            Platform.CurrentActivity.Window.Attributes = attrs;

            IsShowing = true;
        }

        public void Hide()
        {
            if (!IsShowing)
            {
                return;
            }

            WindowManagerLayoutParams attrs = Platform.CurrentActivity.Window.Attributes;

            _orginalFlags = attrs.Flags;

            attrs.Flags |= WindowManagerFlags.Fullscreen;

            Platform.CurrentActivity.Window.Attributes = attrs;

            IsShowing = false;
        }
    }
}
