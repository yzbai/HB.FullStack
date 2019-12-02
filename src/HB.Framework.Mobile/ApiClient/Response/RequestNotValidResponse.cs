using HB.Framework.Common;
using HB.Framework.Common.Mobile;
using System;

namespace HB.Framework.Mobile.ApiClient
{
    public class RequestNotValidResponse<T> : ApiResponse<T> where T : ApiData
    {
        public RequestNotValidResponse()
            : base(400, "", ErrorCode.API_REQUEST_VALIDATE_ERROR) { }

        public RequestNotValidResponse(ISupportValidate supportValidate)
            : base(400, supportValidate?.GetValidateErrorMessage(), ErrorCode.API_REQUEST_VALIDATE_ERROR) { }
    }
}
