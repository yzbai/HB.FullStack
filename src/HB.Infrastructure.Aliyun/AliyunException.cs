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
    }
}
