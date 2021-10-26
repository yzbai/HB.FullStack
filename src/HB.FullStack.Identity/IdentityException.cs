namespace System
{
    public class IdentityException : ErrorCode2Exception
    {
        public IdentityException(ErrorCode errorCode) : base(errorCode)
        {
            
        }

        public IdentityException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
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