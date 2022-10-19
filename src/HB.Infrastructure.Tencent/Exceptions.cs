using System;

namespace HB.Infrastructure.Tencent
{
    internal static class Exceptions
    {
        internal static Exception CapthaError(string appId, string cause)
        {
            TencentException exception = new TencentException(ErrorCodes.CapthaError, cause);

            exception.Data["AppId"] = appId;
            exception.Data["Cause"] = cause;

            return exception;
        }
    }
}