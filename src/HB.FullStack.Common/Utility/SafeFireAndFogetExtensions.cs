using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace System
{
    public static class SafeFireAndFogetExtensions
    {
        [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<Pending>")]
        [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>")]
        public static async void Fire(this Task task, string? message = null, LogLevel logLevel = LogLevel.Error, bool continueOnCapturedContext = false)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex)
            {
                GlobalSettings.MessageExceptionHandler.Invoke(ex, message, logLevel);
            }
        }
    }
}
