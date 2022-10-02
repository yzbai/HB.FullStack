﻿using System;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.WebApi
{
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string?, string?, string?, string?, ErrorCode, Exception?> _logGlobalException = LoggerMessage.Define<string?, string?, string?, string?, ErrorCode>(
            LogLevel.Error,
            ErrorCodes.GlobalExceptionError.ToEventId(),
            "被GlobalExceptionHandler捕捉。Path={Path}, Route={Route}, Query={Query}, Content={Content}, ErrorCode={ErrorCode}"
            );

        public static void LogGlobalException(this ILogger logger, string? path, string? route, string? query, string? content, ErrorCode errorCode, Exception? exception)
        {
            _logGlobalException(logger, path, route, query, content, errorCode, exception);
        }

        public static void LogStarup(this ILogger logger)
        {
            logger.LogInformation("启动 MyColorfulTime.Server.MainApi, 环境: {AspNetCoreEnvironment}, MachineId:{MachineId}",
                EnvironmentUtil.AspNetCoreEnvironment, EnvironmentUtil.MachineId);
        }

        public static void LogCriticalShutDown(this ILogger logger, Exception ex)
        {
            logger.LogCritical(ex, "MyColorfulTime.MainApi 因为没有处理的异常，现在关闭!!!!!!!!!!!!!!!!!!!!!.");
        }
    }
}