namespace System
{
    public class AliyunException : ErrorCodeException
    {


        [Obsolete("DoNotUse")]
        public AliyunException()
        {
        }

        [Obsolete("DoNotUse")]
        public AliyunException(string message) : base(message)
        {
        }

        [Obsolete("DoNotUse")]
        public AliyunException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AliyunException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
