namespace System
{
    public class DatabaseException : ErrorCodeException
    {
        public DatabaseException(ErrorCode errorCode, string cause, Exception? innerException, object? context) : base(errorCode, cause, innerException, context)
        {
        }

        [Obsolete("Do Not Use.")]
        public DatabaseException()
        {
        }

        [Obsolete("Do Not Use.")]
        public DatabaseException(string message) : base(message)
        {
        }

        [Obsolete("Do Not Use.")]
        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public bool ComeFromEngine { get; set; }
    }
}