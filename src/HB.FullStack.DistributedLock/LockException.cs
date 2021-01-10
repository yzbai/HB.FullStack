using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Lock;

namespace System
{
    public class LockException : Exception
    {
        public LockErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public LockException(LockErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public LockException(LockErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public LockException(LockErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
