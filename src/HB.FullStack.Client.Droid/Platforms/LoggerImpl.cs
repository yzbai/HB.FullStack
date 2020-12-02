using Android.Util;
using HB.FullStack.Client.Droid.Platforms;
using HB.FullStack.Client.Platforms;
using System;

[assembly: Xamarin.Forms.Dependency(typeof(LoggerImpl))]
namespace HB.FullStack.Client.Droid.Platforms
{
    public class LoggerImpl : IPlatformLoggerImpl
    {
        private const string _tag = "MyColorfulTime";

        public void Wtf(string message)
        {
            Log.Wtf(_tag, message);
        }

        public void Error(string message)
        {
            Log.Error(_tag, message);
        }

        public void Wtf(Exception exception)
        {
            Log.Wtf(_tag, exception.ToString());
        }

        public void Error(Exception exception)
        {
            Log.Error(_tag, exception.ToString());
        }

        public void Warn(string message)
        {
            Log.Warn(_tag, message);
        }

        public void Info(string message)
        {
            Log.Info(_tag, message);
        }

        public void Debug(string message)
        {
            Log.Debug(_tag, message);
        }

        public void Trace(string message)
        {
            Log.Verbose(_tag, message);
        }
    }
}