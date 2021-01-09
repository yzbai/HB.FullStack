
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.Direct
{
    public class RedisDatabaseException : CommonException
    {
        public RedisDatabaseException()
        {
        }

        public RedisDatabaseException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public RedisDatabaseException(string? message) : base(message)
        {
        }

        public RedisDatabaseException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public RedisDatabaseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public RedisDatabaseException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }
    }
}
