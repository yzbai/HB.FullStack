using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class LogicException : ClientException
    {
        public LogicException(string message) : base(message)
        {
        }

        public LogicException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LogicException()
        {
        }
    }
}
