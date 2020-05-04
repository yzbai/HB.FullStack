#nullable enable

using HB.Framework.Common.Api;
using System;

namespace HB.Framework.Client.Api
{
    public class ApiResponse : ApiResponse<object>
    {

        public ApiResponse(object? data, int httpCode) : base(data, httpCode) { }

        public ApiResponse(int httpCode, string? message, ApiError errorCode) : base(httpCode, message, errorCode) { }

    }

    public class ApiResponse<T> where T : class
    {
        public int HttpCode { get; private set; }

        public string? Message { get; private set; }

        public ApiError ErrCode { get; private set; } = ApiError.FAILED;

        public T? Data { get; set; }

        public ApiResponse(T? data, int httpCode)
        {
            Data = data;
            HttpCode = httpCode;
        }

        public ApiResponse(int httpCode, string? message, ApiError errorCode)
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
            ThrowIf.Null(t, nameof(t));

            ApiResponse rt = new ApiResponse(t.HttpCode, t.Message, t.ErrCode)
            {
                Data = t.Data
            };

            return rt;
        }

        public static implicit operator ApiResponse<T>(ApiResponse v)
        {
            ThrowIf.Null(v, nameof(v));

            ApiResponse<T> rt = new ApiResponse<T>(v.HttpCode, v.Message, v.ErrCode);

            if (v.Data != null)
            {
                rt.Data = (T)v.Data;
            }

            return rt;
        }

        public ApiResponse ToApiResponse()
        {
            ApiResponse rt = new ApiResponse(HttpCode, Message, ErrCode)
            {
                Data = Data
            };

            return rt;
        }
    }
}
