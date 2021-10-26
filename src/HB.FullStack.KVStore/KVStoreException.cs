using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using HB.FullStack.KVStore;

namespace System
{
    public class KVStoreException : ErrorCode2Exception
    {
        public KVStoreException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public KVStoreException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public KVStoreException()
        {
        }

        public KVStoreException(string message) : base(message)
        {
        }

        public KVStoreException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
