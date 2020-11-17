using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentityException : FrameworkException
    {
        public IdentityException(string? message) : base(message)
        {
        }

        public IdentityException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public IdentityException()
        {
        }

        public IdentityException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public IdentityException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public IdentityException(ErrorCode errorCode) : base(errorCode)
        {
        }
    }
}
