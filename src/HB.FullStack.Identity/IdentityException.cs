namespace System
{
    public class IdentityException : ErrorCode2Exception
    {

        [Obsolete("DoNotUse")]
        public IdentityException()
        {
        }

        [Obsolete("DoNotUse")]
        public IdentityException(string message) : base(message)
        {
        }

        [Obsolete("DoNotUse")]
        public IdentityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IdentityException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}