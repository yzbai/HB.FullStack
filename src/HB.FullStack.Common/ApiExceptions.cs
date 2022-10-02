namespace System
{
    public static partial class ApiExceptions
    {

        public static Exception ServerUnkownError(string responseString)
        {
            return new ApiException(ErrorCodes.ServerUnKownError, "Server返回了其他格式的错误表示，赶紧处理", null, new { Response = responseString });
        }

        internal static Exception ApiClientInnerError(string cause, Exception? innerEx, object? context)
        {
            return new ApiException(ErrorCodes.ApiClientInnerError, cause, innerEx, context);
        }

        internal static Exception ServerReturnError(ErrorCode errorCode)
        {
            return new ApiException(errorCode, "Server认为请求无法返回正确");
        }

        internal static Exception ApiModelError(string cause, Exception? innerEx, object? context)
        {
            return new ApiException(ErrorCodes.ApiModelError, cause, innerEx, context);
        }

        internal static Exception ApiAuthenticationError(string cause, Exception? innerEx, object? context)
        {
            return new ApiException(ErrorCodes.ApiAuthenticationError, cause, innerEx, context);
        }

        internal static Exception ApiResourceError(string cause, Exception? innerEx, object? context)
        {
            return new ApiException(ErrorCodes.ApiResourceError, cause, innerEx, context);
        }

        public static Exception ServerNullReturn(string parameter)
        {
            return new ApiException(ErrorCodes.ServerNullReturn, "Server端返回NULL", null, new { Parameter = parameter });
        }
    }
}