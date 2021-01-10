using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using HB.FullStack.KVStore;

namespace System
{
    public class KVStoreException : Exception
    {
        public KVStoreErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public KVStoreException(KVStoreErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public KVStoreException(KVStoreErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public KVStoreException(KVStoreErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
