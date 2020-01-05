
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.Direct
{
    public class RedisDatabaseException : Exception
    {
        public RedisDatabaseException(string message) : base(message)
        {
        }

        public RedisDatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RedisDatabaseException()
        {
        }
    }
}
