namespace System
{
    public class RepositoryException : ErrorCodeException
    {
        [Obsolete("DoNotUse")]
        public RepositoryException()
        {
        }

        [Obsolete("DoNotUse")]
        public RepositoryException(string? cause) : base(cause)
        {
        }

        [Obsolete("DoNotUse")]
        public RepositoryException(string? cause, Exception innerException) : base(cause, innerException)
        {
        }

        public RepositoryException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}