using Android.Util;

using HB.FullStack.XamarinForms.Droid.Platforms;
using HB.FullStack.XamarinForms.Platforms;

using System;

[assembly: Xamarin.Forms.Dependency(typeof(LoggerImpl))]

namespace HB.FullStack.XamarinForms.Droid.Platforms
{
    public class LoggerImpl : ILoggerImpl
    {
        private const string TAG = "HB.FullStack";

        public void Wtf(string message)
        {
            Log.Wtf(TAG, message);
        }

        public void Error(string message)
        {
            Log.Error(TAG, message);
        }

        public void Wtf(Exception exception)
        {
            Log.Wtf(TAG, exception.ToString());
        }

        public void Error(Exception exception)
        {
            Log.Error(TAG, exception.ToString());
        }

        public void Warn(string message)
        {
            Log.Warn(TAG, message);
        }

        public void Info(string message)
        {
            Log.Info(TAG, message);
        }

        public void Debug(string message)
        {
            Log.Debug(TAG, message);
        }

        public void Trace(string message)
        {
            Log.Verbose(TAG, message);
        }
    }
}