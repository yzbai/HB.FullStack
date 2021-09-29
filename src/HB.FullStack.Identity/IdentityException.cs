namespace System
{
    public class IdentityException : ErrorCodeException
    {
        public IdentityException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public IdentityException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}