#nullable enable

using System;

namespace HB.Framework.Common.Api
{
    public class ApiResponse
    {
        public int HttpCode { get; private set; }

        public string? Message { get; private set; }

        public ErrorCode ErrCode { get; private set; } = ErrorCode.OK;

        public bool IsSuccessful { get => HttpCode >= 200 && HttpCode <= 299; }

        public ApiResponse(int httpCode)
        {
            HttpCode = httpCode;
        }

        public ApiResponse(int httpCode, string? message, ErrorCode errorCode) : this(httpCode)
        {
            Message = message;
            ErrCode = errorCode;
        }
    }

    public class ApiResponse<T> : ApiResponse where T : class
    {
        public T? Data { get; set; }

        public ApiResponse(T? data, int httpCode) : base(httpCode)
        {
            Data = data;
        }

        public ApiResponse(int httpCode, string? message, ErrorCode errorCode) : base(httpCode, message, errorCode)
        {
        }
    }
}