using System;
using System.Net.Http;

namespace HB.Framework.Http.SDK
{
    public class Resource<T> where T : ResourceResponse
    {
        public int HttpCode { get; private set; }

        public string Message { get; private set; }

        public ErrorCode ErrCode { get; private set; } = ErrorCode.FAILED;

        public T Response { get; private set; }

        public bool IsSuccessful()
        {
            return ErrCode == ErrorCode.OK;
        }

        public Resource(T resource, int httpCode)
        {
            HttpCode = httpCode;
            Message = null;
            ErrCode = ErrorCode.OK;
            Response = resource;
        }

        public Resource(int httpCode, string message, ErrorCode errorCode)
        {
            HttpCode = httpCode;
            Message = message;
            ErrCode = errorCode;

            Response = null;
        }
    }
}
