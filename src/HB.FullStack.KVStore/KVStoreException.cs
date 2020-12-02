using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.FullStack.KVStore
{
    public class KVStoreException : FrameworkException
    {
        public KVStoreException(ErrorCode errorCode, string entityName, string? message = null, Exception? innerException = null)
            : base(errorCode, $"EntityName:{entityName}, Message:{message}", innerException)
        {


        }

        public KVStoreException()
        {
        }

        public KVStoreException(string? message) : base(message)
        {
        }

        public KVStoreException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
