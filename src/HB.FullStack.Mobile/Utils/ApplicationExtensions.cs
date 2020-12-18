using System;
using HB.FullStack.Client.Base;
using Microsoft.Extensions.Logging;
using Xamarin.Forms;

namespace Xamarin.Forms
{
    public enum UsageType
    {
        PageCreate,
        PageAppearing,
        PageDisappearing
    }

    public static class ApplicationExtensions
    {

        public static void Log(this Application application, LogLevel logLevel, Exception? ex, string? message)
        {
            if (application is BaseApplication)
            {
                BaseApplication.Log(logLevel, ex, message);
            }
        }

        public static void PerformLogin(this Application application)
        {
            if (application is BaseApplication baseApplication)
            {
                baseApplication.PerformLogin();
            }

        }

        public static void LogUsage(this Application application, UsageType usageType, string? name)
        {
            if (application is BaseApplication)
            {
                BaseApplication.Log(LogLevel.Information, null, $"Trace:{usageType}:{name}");
            }
        }

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
                return baseApplication.Environment;
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
