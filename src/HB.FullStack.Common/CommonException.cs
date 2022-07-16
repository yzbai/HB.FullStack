namespace System
{
    public class CommonException : ErrorCode2Exception
    {
        public CommonException(ErrorCode errorCode, string cause, Exception? innerException, object? context) : base(errorCode, cause, innerException, context)
        {
        }
        [Obsolete("Do not use.")]
        public CommonException()
        {
        }

        [Obsolete("Do not use.")]
        public CommonException(string? message) : base(message)
        {
        }

        [Obsolete("Do not use.")]
        public CommonException(string? message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
