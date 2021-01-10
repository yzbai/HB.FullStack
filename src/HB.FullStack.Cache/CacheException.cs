using System;
using System.Runtime.Serialization;

using HB.FullStack.Cache;

namespace System
{
    public class CacheException : Exception
    {
        public CacheErrorCode ErrorCode { get; set; }

        public CacheException(CacheErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public CacheException(CacheErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public CacheException(CacheErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";
    }
}