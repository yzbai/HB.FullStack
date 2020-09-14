using System;
using HB.Framework.Client.Base;
using Microsoft.Extensions.Logging;
using Xamarin.Forms;

namespace Xamarin.Forms
{
    public static class ApplicationExtensions
    {
        public static Action<Exception>? GetExceptionHandler(this Application application)
        {
            if (application is BaseApplication)
            {
                return BaseApplication.ExceptionHandler;
            }

            return null;
        }

        public static string? GetEnvironment(this Application application)
        {
            if (application is BaseApplication baseApplication)
            {
                return baseApplication.GetEnvironment();
            }

            return null;
        }

        public static LogLevel GetMinimumLogLevel(this Application application)
        {
            if (application is BaseApplication baseApplication)
            {
                return baseApplication.MinimumLogLevel;
            }

            return LogLevel.Information;
        }
    }
}
