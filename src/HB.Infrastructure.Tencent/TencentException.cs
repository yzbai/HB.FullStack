using System;
using System.Collections.Generic;
using System.Text;

using HB.Infrastructure.Tencent;

namespace System
{
    public class TencentException : ErrorCodeException
    {
        public TencentException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public TencentException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}
