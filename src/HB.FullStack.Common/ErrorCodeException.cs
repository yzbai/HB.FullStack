namespace System
{
    public sealed class ErrorCode2Exception : Exception
    {
        public ErrorCode2Exception(ErrorCode errorCode) : base(errorCode.Message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode2Exception(ErrorCode errorCode, Exception? innerException) : base(errorCode.Message, innerException)
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