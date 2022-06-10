using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using HB.FullStack.Common.Api;

namespace Microsoft.Extensions.Logging
{
    public static partial class LoggerExtensions
    {
        public static void LogTrace2(
            this ILogger logger,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogTrace(" {message}, [ {lineNumber} in {memeberName} at {sourceFilePath} ] ", message, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogDebug2(
            this ILogger logger,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogDebug(" {message}, [ {lineNumber} in {memeberName} at {sourceFilePath} ] ", message, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogInformation2(
            this ILogger logger,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogInformation(" {message}, [ {lineNumber} in {memeberName} at {sourceFilePath} ] ", message, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogWarning2(
            this ILogger logger,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogWarning(" {message}, [ {lineNumber} in {memeberName} at {sourceFilePath} ] ", message, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogError2(
            this ILogger logger,
            Exception exception,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogError(exception, " {message}, [ {lineNumber} in {memeberName} at {sourceFilePath} ] ", message, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogError2(
            this ILogger logger,
            Exception exception,
            ApiRequest request,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogError(exception, " {request}, [ {lineNumber} in {memeberName} at {sourceFilePath} ] ", SerializeUtil.ToJson(request), sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogCritical2(
            this ILogger logger,
            Exception? exception,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogCritical(exception, " {message}, [ {lineNumber} in {memeberName} at {sourceFilePath} ] ", message, sourceLineNumber, memberName, sourceFilePath);
        }
    }
}