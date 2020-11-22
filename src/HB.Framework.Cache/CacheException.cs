using System;

namespace System
{
    public class CacheException : FrameworkException
    {
        public CacheException(string? message) : base(message)
        {
        }

        public CacheException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public CacheException()
        {
        }

        public CacheException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public CacheException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public CacheException(ErrorCode errorCode) : base(errorCode)
        {
        }
    }
}