namespace System
{
    public class SmsException : ErrorCode2Exception
    {
        public SmsException(ErrorCode errorCode, string cause, Exception? innerException, object? context) : base(errorCode, cause, innerException, context)
        {
        }

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
    }
}
