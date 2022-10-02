namespace System
{
    public class CacheException : ErrorCodeException
    {
        public CacheException(ErrorCode errorCode, string cause, Exception? innerException, object? context) : base(errorCode, cause, innerException, context)
        {
        }

        [Obsolete("Do not use.")]
        public CacheException()
        {
        }

        [Obsolete("Do not use.")]
        public CacheException(string message) : base(message)
        {
        }

        [Obsolete("Do not use.")]
        public CacheException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}