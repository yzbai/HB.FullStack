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
    }
}