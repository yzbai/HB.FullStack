using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Mobile.Logger
{
    public interface IRemoteLoggingService
    {
        Task LogAsync(LogLevel logLevel, Exception? ex, string? message);
    }
}
