using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.WebApi
{
    public static class WebApiErrorCodes
    {
        public static ErrorCode DataProtectionCertNotFound { get; set; } = new ErrorCode(nameof(DataProtectionCertNotFound), "");
        public static ErrorCode JwtEncryptionCertNotFound { get; set; } = new ErrorCode(nameof(JwtEncryptionCertNotFound), "");
        public static ErrorCode StartupError { get; set; } = new ErrorCode(nameof(StartupError), "");
        public static ErrorCode DatabaseInitLockError { get; set; } = new ErrorCode(nameof(DatabaseInitLockError), "");

        //public static ErrorCode ExceptionHandlerPathFeatureNull { get; } = new ErrorCode(nameof(ExceptionHandlerPathFeatureNull), "");

        //public static ErrorCode ServerUnKownNonErrorCodeError { get; } = new ErrorCode(nameof(ServerUnKownNonErrorCodeError), "");

        public static ErrorCode ServerInternalError { get; } = new ErrorCode(nameof(ServerInternalError), "服务器内部运行错误");

        public static ErrorCode GlobalExceptionError { get; } = new ErrorCode(nameof(GlobalExceptionError), "");
    }

    public static class WebApiExceptions
    {
        public static Exception DatabaseInitLockError(IEnumerable<string> databases)
        {
            WebApiException exception = new WebApiException(WebApiErrorCodes.DatabaseInitLockError, nameof(DatabaseInitLockError));

            exception.Data["Databases"] = databases;

            return exception;
        }

        public static Exception StartupError(object? value, string cause)
        {
            WebApiException exception = new WebApiException(WebApiErrorCodes.StartupError, nameof(StartupError));

            exception.Data["Value"] = value;
            exception.Data["Cause"] = cause;

            return exception;
        }
    }

    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string?, string?, string?, string?, ErrorCode, Exception?> _logGlobalException = LoggerMessage.Define<string?, string?, string?, string?, ErrorCode>(
            LogLevel.Error,
            WebApiErrorCodes.GlobalExceptionError.ToEventId(),
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