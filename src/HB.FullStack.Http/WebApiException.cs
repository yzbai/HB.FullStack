using System;

namespace HB.FullStack.WebApi
{
    public class WebApiException : ErrorCodeException
    {

        [Obsolete("DoNotUse")]
        public WebApiException()
        {
        }
        [Obsolete("DoNotUse")]
        public WebApiException(string message) : base(message)
        {
        }
        [Obsolete("DoNotUse")]
        public WebApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WebApiException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
