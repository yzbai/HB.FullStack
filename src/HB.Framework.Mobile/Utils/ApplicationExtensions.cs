using System;
using HB.Framework.Client.Base;
using Xamarin.Forms;

namespace Xamarin.Forms
{
    public static class ApplicationExtensions
    {
        public static Action<Exception>? GetUIExceptionHandler(this Application application)
        {
            if (application is BaseApplication baseApplication)
            {
                return baseApplication.UIExceptionHandler;
            }

            return null;
        }
    }
}
