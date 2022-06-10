using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class ClientException : ErrorCode2Exception
    {
        public ClientException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public ClientException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public ClientException()
        {
        }

        public ClientException(string message) : base(message)
        {
        }

        public ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
