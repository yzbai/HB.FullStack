using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class MobileException : ErrorCode2Exception
    {
        public MobileException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public MobileException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}
