using System;
using System.Runtime.Serialization;

using HB.FullStack.Cache;

namespace System
{
    public class CacheException : ErrorCode2Exception
    {
        public CacheException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public CacheException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public CacheException()
        {
        }

        public CacheException(string message) : base(message)
        {
        }

        public CacheException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}