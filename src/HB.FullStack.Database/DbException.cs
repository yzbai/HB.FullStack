namespace System
{
    public class DbException : ErrorCodeException
    {
        public DbException(ErrorCode errorCode, string cause, Exception? innerException, object? context) : base(errorCode, cause, innerException, context)
        {
        }

        [Obsolete("Do Not Use.")]
        public DbException()
        {
        }

        [Obsolete("Do Not Use.")]
        public DbException(string message) : base(message)
        {
        }

        [Obsolete("Do Not Use.")]
        public DbException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public bool ComeFromEngine { get; set; }
    }
}