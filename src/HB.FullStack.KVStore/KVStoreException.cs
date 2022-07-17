namespace System
{
    public class KVStoreException : ErrorCode2Exception
    {


        [Obsolete("DoNotUse")]
        public KVStoreException()
        {
        }

        [Obsolete("DoNotUse")]
        public KVStoreException(string message) : base(message)
        {
        }

        [Obsolete("DoNotUse")]
        public KVStoreException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public KVStoreException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
