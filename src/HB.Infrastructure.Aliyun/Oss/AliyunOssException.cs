using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class AliyunOssException : ServerException
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

        public AliyunOssException(ServerErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public AliyunOssException(ServerErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public AliyunOssException(ServerErrorCode errorCode) : base(errorCode)
        {
        }
    }
}
