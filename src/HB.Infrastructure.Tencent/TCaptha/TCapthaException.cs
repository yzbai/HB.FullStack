using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Tencent
{
    public class TCapthaException : CommonException
    {
        public TCapthaException()
        {
        }

        public TCapthaException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public TCapthaException(string? message) : base(message)
        {
        }

        public TCapthaException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public TCapthaException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public TCapthaException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }
    }
}
