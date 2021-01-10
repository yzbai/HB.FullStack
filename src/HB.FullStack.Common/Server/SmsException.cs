using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class SmsException : Exception
    {
        public SmsErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public SmsException(SmsErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public SmsException(SmsErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public SmsException(SmsErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
