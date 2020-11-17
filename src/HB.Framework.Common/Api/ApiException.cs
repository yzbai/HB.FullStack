namespace System
{
    public class ApiException : FrameworkException
    {
        public ApiException(string? message) : base(message)
        {
        }

        public ApiException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public ApiException()
        {
        }

        public ApiException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public ApiException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public ApiException(ErrorCode errorCode) : base(errorCode)
        {
        }
    }
}