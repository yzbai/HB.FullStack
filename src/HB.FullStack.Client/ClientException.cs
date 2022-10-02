namespace System
{
    public class ClientException : ErrorCodeException
    {
        [Obsolete("DoNotUse")]
        public ClientException()
        {
        }

        [Obsolete("DoNotUse")]
        public ClientException(string message) : base(message)
        {
        }

        [Obsolete("DoNotUse")]
        public ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ClientException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
