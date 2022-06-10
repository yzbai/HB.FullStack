using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class SmsException : ErrorCode2Exception
    {
        public SmsException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public SmsException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public SmsException()
        {
        }

        public SmsException(string message) : base(message)
        {
        }

        public SmsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
