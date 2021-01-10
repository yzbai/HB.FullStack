#nullable enable

using System.Runtime.Serialization;

namespace System
{
    public class CommonException2 : Exception
    {

        public CommonErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public CommonException2(CommonErrorCode errorCode):base()
        {
            ErrorCode = errorCode;
        }

        public CommonException2(CommonErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public CommonException2(CommonErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}

#nullable restore