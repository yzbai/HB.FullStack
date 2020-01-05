using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsException : Exception
    {
        public AliyunSmsException()
        {
        }

        public AliyunSmsException(string message) : base(message)
        {
        }

        public AliyunSmsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
