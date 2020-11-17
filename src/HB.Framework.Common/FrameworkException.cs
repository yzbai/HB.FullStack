#nullable enable

namespace System
{
    public class FrameworkException : Exception
    {
        public FrameworkException(ErrorCode errorCode, string? message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public FrameworkException(ErrorCode errorCode, string? message, Exception? innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public FrameworkException(ErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; set; }

        public FrameworkException()
        {
        }

        public FrameworkException(string? message) : base(message)
        {
        }

        public FrameworkException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

#nullable restore