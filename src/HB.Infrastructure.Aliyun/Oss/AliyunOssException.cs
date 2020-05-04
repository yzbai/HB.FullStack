using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class AliyunOssException : FrameworkException
    {
        public override FrameworkExceptionType ExceptionType { get => FrameworkExceptionType.AliyunOss; }

        public AliyunOssException(string message) : base(message)
        {
        }

        public AliyunOssException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AliyunOssException()
        {
        }
    }
}
