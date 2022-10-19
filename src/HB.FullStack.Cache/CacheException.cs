namespace System
{
    public class CacheException : ErrorCodeException
    {
        [Obsolete("DoNotUse")]
        public CacheException()
        {
        }

        [Obsolete("DoNotUse")]
        public CacheException(string? cause) : base(cause)
        {
        }

        [Obsolete("DoNotUse")]
        public CacheException(string? cause, Exception innerException) : base(cause, innerException)
        {
        }

        public CacheException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
