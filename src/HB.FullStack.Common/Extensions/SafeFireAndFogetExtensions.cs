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
#pragma warning disable CA1030 // Use events where appropriate
        public static async void Fire(this Task task, string? message = null, LogLevel logLevel = LogLevel.Error, bool continueOnCapturedContext = false)
#pragma warning restore CA1030 // Use events where appropriate
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                GlobalSettings.MessageExceptionHandler.Invoke(ex, message, logLevel);
            }
        }
    }
}
