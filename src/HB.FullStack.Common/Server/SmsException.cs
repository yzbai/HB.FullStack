namespace System
{
    public class SmsException : ErrorCodeException
    {


        [Obsolete("Do not use.")]
        public SmsException()
        {
        }

        [Obsolete("Do not use.")]
        public SmsException(string message) : base(message)
        {
        }

        [Obsolete("Do not use.")]
        public SmsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SmsException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
