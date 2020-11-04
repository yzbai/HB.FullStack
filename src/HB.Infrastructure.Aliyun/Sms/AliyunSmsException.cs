using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsException : ServerException
    {

        public AliyunSmsException()
        {
        }

        public AliyunSmsException(string? message) : base(message)
        {
        }

        public AliyunSmsException(ServerErrorCode errorCode) : base(errorCode)
        {
        }

        public AliyunSmsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public AliyunSmsException(ServerErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public AliyunSmsException(ServerErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }
    }
}
