namespace System
{
    public class MauiException : ErrorCode2Exception
    {
        [Obsolete("DoNotUse")]
        public MauiException()
        {
        }

        [Obsolete("DoNotUse")]
        public MauiException(string? cause) : base(cause)
        {
        }

        [Obsolete("DoNotUse")]
        public MauiException(string? cause, Exception innerException) : base(cause, innerException)
        {
        }

        public MauiException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
