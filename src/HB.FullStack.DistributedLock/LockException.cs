namespace System
{
    public class LockException : ErrorCode2Exception
    {


        [Obsolete("DoNotUse")]
        public LockException()
        {
        }

        [Obsolete("DoNotUse")]
        public LockException(string message) : base(message)
        {
        }

        [Obsolete("DoNotUse")]
        public LockException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LockException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
