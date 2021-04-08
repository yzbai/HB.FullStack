using System;
using System.Collections.Generic;

namespace HB.FullStack.WebApi
{
    public static class WebApiErrorCodes
    {
        public static ErrorCode DataProtectionCertNotFound { get; set; } = new ErrorCode(ErrorCodeStartIds.WEB_API + 0, nameof(DataProtectionCertNotFound), "");
        public static ErrorCode JwtEncryptionCertNotFound { get; set; } = new ErrorCode(ErrorCodeStartIds.WEB_API + 1, nameof(JwtEncryptionCertNotFound), "");
        public static ErrorCode StartupError { get; set; } = new ErrorCode(ErrorCodeStartIds.WEB_API + 2, nameof(StartupError), "");
        public static ErrorCode DatabaseInitLockError { get; set; } = new ErrorCode(ErrorCodeStartIds.WEB_API + 3, nameof(DatabaseInitLockError), "");
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
}