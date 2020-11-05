using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class AliyunOssException : FrameworkException
    {

        public AliyunOssException(string? message) : base(message)
        {
        }

        public AliyunOssException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public AliyunOssException()
        {
        }

        public AliyunOssException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public AliyunOssException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public AliyunOssException(ErrorCode errorCode) : base(errorCode)
        {
        }
    }
}
