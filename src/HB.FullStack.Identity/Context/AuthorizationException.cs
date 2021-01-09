using System;

namespace HB.FullStack.Identity
{
    public class AuthorizationException : CommonException
    {
        public AuthorizationException(string? message) : base(message)
        {
        }

        public AuthorizationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public AuthorizationException()
        {
        }

        public AuthorizationException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public AuthorizationException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public AuthorizationException(ErrorCode errorCode) : base(errorCode)
        {
        }
    }
}
