namespace System
{
    public class IdentityException : ErrorCode2Exception
    {
        public IdentityException(ErrorCode errorCode, string cause) : base(errorCode, cause)
        {

        }

        public IdentityException(ErrorCode errorCode, string cause, Exception? innerException) : base(errorCode, cause, innerException)
        {
        }

        public IdentityException()
        {
        }

        public IdentityException(string message) : base(message)
        {
        }

        public IdentityException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}