using HB.Framework.Common.Api;

namespace HB.Framework.Mobile.ApiClient
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
    }
}
