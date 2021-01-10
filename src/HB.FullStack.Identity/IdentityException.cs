using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.FullStack.Identity
{
    public class IdentityException : Exception
    {
        public IdentityErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public IdentityException(IdentityErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public IdentityException(IdentityErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public IdentityException(IdentityErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
