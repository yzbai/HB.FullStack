using HB.Framework.Common;
using System;

namespace HB.Framework.Http.SDK
{
    public class RequestValidateErrorResponse<T> : Resource<T> where T: ResourceResponse
    {
        public RequestValidateErrorResponse()
            : base(400, "", ErrorCode.API_REQUEST_VALIDATE_ERROR) { }

        public RequestValidateErrorResponse(ISupportValidate supportValidate)
            : base(400, supportValidate?.GetValidateErrorMessage(), ErrorCode.API_REQUEST_VALIDATE_ERROR) { }
    }
}
