using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class MobileException : Exception
    {
        public MobileErrorCode ErrorCode { get; set; }

        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public MobileException(MobileErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public MobileException(MobileErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public MobileException(MobileErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
