using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Tencent
{
    public class TCapthaException : FrameworkException
    {
        public override FrameworkExceptionType ExceptionType { get => FrameworkExceptionType.TCaptha; }

        public TCapthaException(string? message) : base(message)
        {
        }

        public TCapthaException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public TCapthaException()
        {
        }
    }
}
