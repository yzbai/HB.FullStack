using System;
using System.Collections.Generic;
using System.Text;

using HB.Infrastructure.Tencent;

namespace System
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<Pending>")]
    public class TencentException : Exception
    {
        public TencentErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public TencentException(TencentErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public TencentException(TencentErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public TencentException(TencentErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
