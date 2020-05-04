using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsException : FrameworkException
    {
        public override FrameworkExceptionType ExceptionType { get => FrameworkExceptionType.AliyunSms;}

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
