#nullable enable

using System;
using System.Collections.Generic;


namespace HB.FullStack.Common.Api
{
    public class ApiError : ApiResource
    {
        public ApiErrorCode ErrorCode { get; set; }

        public string? Message { get; set; }

        public IDictionary<string, IEnumerable<string>>? ModelStates { get; set; } = new Dictionary<string, IEnumerable<string>>();

        public ApiError()
        {
        }

        public ApiError(ApiErrorCode code, string? message = null)
        {
            ErrorCode = code;
            Message = message ?? code.ToString();
        }

        public ApiError(ApiErrorCode code, IDictionary<string, IEnumerable<string>> modelStates) : this(code: code, message: null)
        {
            ModelStates = modelStates;
        }

        public ApiError(ApiException ex)
        {
            ErrorCode = ex.ErrorCode;
            Message = ex.Message;
            ModelStates = ex.ModelStates;
        }
    }
}