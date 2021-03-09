using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using HB.FullStack.Common.Api;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        private const string MESSAGE_SUFFIX_TEMPLATE = ", [{lineNumber} in {memeberName} at {sourceFilePath}] ";

        public static void LogTrace2(
            this ILogger logger,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogTrace(message + MESSAGE_SUFFIX_TEMPLATE, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogDebug2(
            this ILogger logger, 
            string message, 
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "", 
            [CallerFilePath] string sourceFilePath = "") 
        {
            logger.LogDebug(message + MESSAGE_SUFFIX_TEMPLATE, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogInformation2(
            this ILogger logger,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogInformation(message + MESSAGE_SUFFIX_TEMPLATE, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogWarning2(
            this ILogger logger,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogWarning(message + MESSAGE_SUFFIX_TEMPLATE, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogError2(
            this ILogger logger, 
            Exception exception, 
            string message, 
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogError(exception, message + MESSAGE_SUFFIX_TEMPLATE, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogError2(
            this ILogger logger,
            Exception exception,
            ApiRequest request,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogError(exception, request.ToDebugInfo() + MESSAGE_SUFFIX_TEMPLATE, sourceLineNumber, memberName, sourceFilePath);
        }

        public static void LogCritical2(
            this ILogger logger,
            Exception exception,
            string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogCritical(exception, message + MESSAGE_SUFFIX_TEMPLATE, sourceLineNumber, memberName, sourceFilePath);
        }
    }
}
