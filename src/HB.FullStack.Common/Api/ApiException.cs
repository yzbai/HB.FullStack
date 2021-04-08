using System.Collections.Generic;
using System.Net;

using HB.FullStack.Common.Api;

namespace System
{
    public class ApiException : ErrorCodeException
    {
        public ApiException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public ApiException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}