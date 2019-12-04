using HB.Framework.Common;
using HB.Framework.Common.Api;

namespace HB.Framework.Mobile.ApiClient
{
    public class RequestNotValidResponse : ApiResponse
    {
        public RequestNotValidResponse()
            : base(400, "", ApiError.API_REQUEST_VALIDATE_ERROR) { }

        public RequestNotValidResponse(ISupportValidate supportValidate)
            : base(400, supportValidate?.GetValidateErrorMessage(), ApiError.API_REQUEST_VALIDATE_ERROR) { }
    }
}
