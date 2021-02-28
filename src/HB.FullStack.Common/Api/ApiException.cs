using System.Collections.Generic;
using System.Net;

using HB.FullStack.Common.Api;

namespace System
{
    public class ApiException : Exception
    {
        public HttpStatusCode? HttpCode { get; set; }

        public ApiErrorCode ErrorCode { get; }

        public ApiException(ApiErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }

        public ApiException(ApiErrorCode errorCode, string? message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ApiException(ApiErrorCode errorCode, string? message, Exception? innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public IDictionary<string, IEnumerable<string>>? ModelStates { get; set; }

        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";
    }
}