using HB.Framework.Common.Mobile;
using System;

namespace HB.Framework.Mobile.ApiClient
{
    public class ApiResponse
    {
        public int HttpCode { get; private set; }

        public string Message { get; private set; }

        public ErrorCode ErrCode { get; private set; } = ErrorCode.FAILED;

        public bool IsSuccessful()
        {
            return ErrCode == ErrorCode.OK;
        }

        public ApiResponse(int httpCode)
        {
            HttpCode = httpCode;
            Message = null;
            ErrCode = ErrorCode.OK;
        }

        public ApiResponse(int httpCode, string message, ErrorCode errorCode)
        {
            HttpCode = httpCode;
            Message = message;
            ErrCode = errorCode;
        }
    }

    public class ApiResponse<T> : ApiResponse where T : ApiData
    {
        public T Resource { get; private set; }

        public ApiResponse(T resource, int httpCode) : base(httpCode)
        {
            Resource = resource;
        }

        public ApiResponse(int httpCode, string message, ErrorCode errorCode) : base(httpCode, message, errorCode)
        {
            Resource = null;
        }
    }
}
