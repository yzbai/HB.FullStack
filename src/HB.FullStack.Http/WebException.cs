using System;

namespace System
{
    public class WebException : ErrorCodeException
    {

        [Obsolete("DoNotUse")]
        public WebException()
        {
        }
        [Obsolete("DoNotUse")]
        public WebException(string message) : base(message)
        {
        }
        [Obsolete("DoNotUse")]
        public WebException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WebException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
