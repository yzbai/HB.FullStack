using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Tencent
{
    public class TCapthaException : ServerException
    {
        public TCapthaException()
        {
        }

        public TCapthaException(ServerErrorCode errorCode) : base(errorCode)
        {
        }

        public TCapthaException(string? message) : base(message)
        {
        }

        public TCapthaException(ServerErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public TCapthaException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public TCapthaException(ServerErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }
    }
}
