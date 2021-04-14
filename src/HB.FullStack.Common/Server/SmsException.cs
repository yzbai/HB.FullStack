using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class SmsException : ErrorCodeException
    {
        public SmsException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public SmsException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}
