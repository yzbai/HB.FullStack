using Android.Util;
using HB.FullStack.XamarinForms.Droid.Platforms;
using HB.FullStack.XamarinForms.Platforms;
using System;

[assembly: Xamarin.Forms.Dependency(typeof(LoggerImpl))]
namespace HB.FullStack.XamarinForms.Droid.Platforms
{
    public class LoggerImpl : ILoggerImpl
    {
        private const string _tAG = "HB.FullStack";

        public void Wtf(string message)
        {
            Log.Wtf(_tAG, message);
        }

        public void Error(string message)
        {
            Log.Error(_tAG, message);
        }

        public void Wtf(Exception exception)
        {
            Log.Wtf(_tAG, exception.ToString());
        }

        public void Error(Exception exception)
        {
            Log.Error(_tAG, exception.ToString());
        }

        public void Warn(string message)
        {
            Log.Warn(_tAG, message);
        }

        public void Info(string message)
        {
            Log.Info(_tAG, message);
        }

        public void Debug(string message)
        {
            Log.Debug(_tAG, message);
        }

        public void Trace(string message)
        {
            Log.Verbose(_tAG, message);
        }
    }
}