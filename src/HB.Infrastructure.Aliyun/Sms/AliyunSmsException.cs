using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsException : CommonException
    {

       
        public AliyunSmsException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public AliyunSmsException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public AliyunSmsException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }
    }
}
