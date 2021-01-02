using HB.Component.Authorization.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Component.Authorization
{
    public class AuthorizationException : FrameworkException
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
