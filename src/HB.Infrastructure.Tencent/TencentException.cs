using System;
using System.Collections.Generic;
using System.Text;

using HB.Infrastructure.Tencent;

namespace System
{
    public class TencentException : ErrorCode2Exception
    {
        public TencentException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public TencentException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public TencentException()
        {
        }

        public TencentException(string message) : base(message)
        {
        }

        public TencentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
