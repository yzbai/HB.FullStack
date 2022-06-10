using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.Infrastructure.Aliyun;

namespace System
{
    public class AliyunException : ErrorCode2Exception
    {
        public AliyunException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public AliyunException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public AliyunException()
        {
        }

        public AliyunException(string message) : base(message)
        {
        }

        public AliyunException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
