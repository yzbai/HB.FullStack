using Microsoft.Maui.ApplicationModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Utils
{
    public static class PlatformUtil
    {
        //TODO: 添加其他平台实现
        public static bool IsFullScreen
        {
            get
            {
#if ANDROID
                bool? fullScreen = Platform.CurrentActivity?.Window?.Attributes?.Flags.HasFlag(Android.Views.WindowManagerFlags.Fullscreen);
                return fullScreen.HasValue && fullScreen.Value;
#elif IOS
                
                return false;
#elif WINDOWS
                return false;
#else
                return false;
#endif


            }
            set
            {
#if ANDROID
                bool? fullScreen = Platform.CurrentActivity?.Window?.Attributes?.Flags.HasFlag(Android.Views.WindowManagerFlags.Fullscreen);

                if (!fullScreen.HasValue)
                {
                    return;
                }

                if(fullScreen == value)
                {
                    return;
                }

                if(value)
                {
                    Platform.CurrentActivity?.Window?.AddFlags(Android.Views.WindowManagerFlags.Fullscreen);
                }
                else
                {
                    Platform.CurrentActivity?.Window?.ClearFlags(Android.Views.WindowManagerFlags.Fullscreen);
                }

#elif iOS
#elif WINDOWS
#else
                
#endif
            }
        }
    }
}
