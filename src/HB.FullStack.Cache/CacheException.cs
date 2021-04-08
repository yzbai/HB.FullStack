using System;
using System.Runtime.Serialization;

using HB.FullStack.Cache;

namespace System
{
    public class CacheException : ErrorCodeException
    {
        public CacheException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public CacheException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}