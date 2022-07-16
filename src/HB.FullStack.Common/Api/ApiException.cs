namespace System
{
    public class ApiException : ErrorCode2Exception
    {
        public ApiException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }

        [Obsolete("不要用")]
        public ApiException()
        {
        }

        [Obsolete("不要用")]
        public ApiException(string message) : base(message)
        {
        }

        [Obsolete("不要用")]
        public ApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
