using HB.Framework.Common;
using HB.Framework.Common.Api;

namespace HB.Framework.Client.ApiClient
{
    public class RequestNotValidResponse : ApiResponse
    {
        public RequestNotValidResponse()
            : base(400, "", ApiError.ApiRequestValidateError) { }

        public RequestNotValidResponse(ISupportValidate supportValidate)
            : base(400, supportValidate?.GetValidateErrorMessage(), ApiError.ApiRequestValidateError) { }
    }
}
