namespace System
{
    public class MauiException : ErrorCode2Exception
    {
        public MauiException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public MauiException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public MauiException()
        {
        }

        public MauiException(string message) : base(message)
        {
        }

        public MauiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
