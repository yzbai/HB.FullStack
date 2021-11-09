namespace System
{
    public class ErrorCode2Exception : Exception
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
                return (ErrorCode?)Data[nameof(ErrorCode)]!;
            }
            private set
            {
                Data[nameof(ErrorCode)] = value;
            }
        }

        public ErrorCode2Exception()
        {
        }

        public ErrorCode2Exception(string? message) : base(message)
        {
        }

        public ErrorCode2Exception(string? message, Exception innerException) : base(message, innerException)
        {
        }
    }
}