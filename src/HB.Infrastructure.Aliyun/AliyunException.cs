using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.Infrastructure.Aliyun;

namespace System
{
    public class AliyunException : Exception
    {
        public AliyunErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public AliyunException(AliyunErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public AliyunException(AliyunErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public AliyunException(AliyunErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
