using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HB.Framework.Client.Services
{
    public interface IRemoteLoggingService
    {
        Task LogAsync(LogLevel logLevel, Exception? ex, string? message);
    }
}
