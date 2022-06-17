using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HB.FullStack.WebApi
{
    public static class WebApiErrorCodes
    {
        public static ErrorCode DataProtectionCertNotFound { get; set; } = new ErrorCode(nameof(DataProtectionCertNotFound), "");
        public static ErrorCode JwtEncryptionCertNotFound { get; set; } = new ErrorCode(nameof(JwtEncryptionCertNotFound), "");
        public static ErrorCode StartupError { get; set; } = new ErrorCode(nameof(StartupError), "");
        public static ErrorCode DatabaseInitLockError { get; set; } = new ErrorCode(nameof(DatabaseInitLockError), "");

        public static ErrorCode ExceptionHandlerPathFeatureNull { get; } = new ErrorCode(nameof(ExceptionHandlerPathFeatureNull), "");

        public static ErrorCode ServerUnKownNonErrorCodeError { get; } = new ErrorCode(nameof(ServerUnKownNonErrorCodeError), "");

        public static ErrorCode GlobalExceptionError { get; } = new ErrorCode(nameof(GlobalExceptionError), "");
    }

    public static class WebApiExceptions
    {
        public static Exception DatabaseInitLockError(IEnumerable<string> databases)
        {
            WebApiException exception = new WebApiException(WebApiErrorCodes.DatabaseInitLockError);

            exception.Data["Databases"] = databases;

            return exception;
        }

        public static Exception StartupError(object? value, string cause)
        {
            WebApiException exception = new WebApiException(WebApiErrorCodes.StartupError);

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
            logger.LogInformation($"启动 MyColorfulTime.Server.MainApi, 环境: {EnvironmentUtil.AspNetCoreEnvironment}, MachineId:{EnvironmentUtil.MachineId}");
        }

        public static void LogCriticalShutDown(this ILogger logger, Exception ex)
        {
            logger.LogCritical(ex, "MyColorfulTime.MainApi 因为没有处理的异常，现在关闭!!!!!!!!!!!!!!!!!!!!!.");
        }
    }
}