using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using HB.FullStack.Identity;

namespace System
{
    public class IdentityException : ErrorCodeException
    {
        public IdentityException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public IdentityException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}
