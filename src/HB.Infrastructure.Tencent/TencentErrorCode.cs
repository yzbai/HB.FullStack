using System;

namespace HB.Infrastructure.Tencent
{
    internal static class TencentErrorCodes
    {
        public static ErrorCode CapthaError { get; set; } = new ErrorCode(nameof(CapthaError), "");
    }

    internal static class Exceptions
    {
        internal static Exception CapthaError(string appId, string cause)
        {
            TencentException exception = new TencentException(TencentErrorCodes.CapthaError);

            exception.Data["AppId"] = appId;
            exception.Data["Cause"] = cause;

            return exception;
        }
    }
}