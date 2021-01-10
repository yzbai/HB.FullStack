using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class ClientException : FrameworkException
    {
        public ClientException()
        {
        }

        public ClientException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public ClientException(string? message) : base(message)
        {
        }

        public ClientException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public ClientException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public ClientException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }
    }
}
