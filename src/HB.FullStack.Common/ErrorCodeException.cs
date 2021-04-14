namespace System
{
    public class ErrorCodeException : Exception
    {
        public ErrorCodeException(ErrorCode errorCode) : base(errorCode.Message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCodeException(ErrorCode errorCode, Exception? innerException) : base(errorCode.Message, innerException)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode
        {
            get
            {
                return (ErrorCode)Data[nameof(ErrorCode)];
            }
            private set
            {
                Data[nameof(ErrorCode)] = value;
            }
        }
    }
}