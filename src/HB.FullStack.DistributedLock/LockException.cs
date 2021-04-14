using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Lock;

namespace System
{
    public class LockException : ErrorCodeException
    {
        public LockException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public LockException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}
