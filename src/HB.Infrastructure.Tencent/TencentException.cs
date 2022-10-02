namespace System
{
    public class TencentException : ErrorCodeException
    {

        [Obsolete("DoNotUse")]
        public TencentException()
        {
        }
        [Obsolete("DoNotUse")]
        public TencentException(string message) : base(message)
        {
        }
        [Obsolete("DoNotUse")]
        public TencentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TencentException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
