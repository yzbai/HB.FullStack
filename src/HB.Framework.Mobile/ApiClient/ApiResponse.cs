using HB.Framework.Common.Api;
using System;

namespace HB.Framework.Client.ApiClient
{
    public class ApiResponse : ApiResponse<object>
    {

        public ApiResponse(object data, int httpCode) : base(data, httpCode) { }

        public ApiResponse(int httpCode, string message, ApiError errorCode) : base(httpCode, message, errorCode) { }

    }

    public class ApiResponse<T>
    {
        public int HttpCode { get; private set; }

        public string Message { get; private set; } = null;

        public ApiError ErrCode { get; private set; } = ApiError.FAILED;

        public T Data { get; set; }

        public ApiResponse(T data, int httpCode)
        {
            Data = data;
            HttpCode = httpCode;
        }

        public ApiResponse(int httpCode, string message, ApiError errorCode)
        {
            HttpCode = httpCode;
            Message = message;
            ErrCode = errorCode;
        }

        public bool IsSuccessful()
        {
            return HttpCode >= 200 && HttpCode <= 299;
        }

        public static implicit operator ApiResponse(ApiResponse<T> t)
        {
            ApiResponse rt = new ApiResponse(t.HttpCode, t.Message, t.ErrCode) {
                Data = t.Data
            };

            return rt;
        }

        public static implicit operator ApiResponse<T>(ApiResponse v)
        {
            ApiResponse<T> rt = new ApiResponse<T>(v.HttpCode, v.Message, v.ErrCode) {
                Data = (T)v.Data
            };

            return rt;
        }
    }
}
