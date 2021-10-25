namespace System
{
    public class ApiException : ErrorCode2Exception
    {
        public ApiException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public ApiException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public ApiException()
        {
        }

        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
