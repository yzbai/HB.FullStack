using HB.FullStack.Common.Api;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static partial class ErrorCodes
    {
        public static readonly ErrorCode CertNotInPackage = new ErrorCode(ErrorCodeStartIds.COMMON + 1, nameof(CertNotInPackage), "证书没有打包在程序里，将试图在服务器中寻找");
        public static readonly ErrorCode CertNotFound = new ErrorCode(ErrorCodeStartIds.COMMON + 2, nameof(CertNotFound),"没有找到证书");
    }

    public static partial class LoggerExtensions
    {
        private static readonly Action<ILogger, string?, Exception?> _logCerNotInPackage = LoggerMessage.Define<string?>(
            LogLevel.Warning,
            ErrorCodes.CertNotInPackage.ToEventId(), 
            "证书 {FullPath} 没有打包，将试图在服务器中寻找");

        public static void LogCertNotInPackage(this ILogger logger, string? fullPath)
        {
            _logCerNotInPackage(logger, fullPath, null);
        }
    }
    
    public static partial class Exceptions
    {
        public static ErrorCodeException CertNotFound(string? subject, string? fullPath)
        {
            ErrorCodeException ex = new ErrorCodeException(ErrorCodes.CertNotFound);
            ex.Data["Subject"] = subject;
            ex.Data["FullPath"] = fullPath;

            return ex;
        }

        
    }
}
