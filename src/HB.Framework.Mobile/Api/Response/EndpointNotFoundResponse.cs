using HB.Framework.Common;
using HB.Framework.Common.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Client.Api
{
    public class EndpointNotFoundResponse : ApiResponse
    {
        public EndpointNotFoundResponse()
            : base(400, "", ApiError.EndpointNotFound) { }

        public EndpointNotFoundResponse(ISupportValidate supportValidate)
            : base(400, supportValidate?.GetValidateErrorMessage(), ApiError.EndpointNotFound) { }
    }
}
