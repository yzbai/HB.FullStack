namespace System
{
    public class RepositoryException : ErrorCode2Exception
    {


        [Obsolete("DoNotUse")]
        public RepositoryException()
        {
        }

        [Obsolete("DoNotUse")]
        public RepositoryException(string message) : base(message)
        {
        }

        [Obsolete("DoNotUse")]
        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RepositoryException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
