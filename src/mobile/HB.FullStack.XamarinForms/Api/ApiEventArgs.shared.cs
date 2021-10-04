using HB.FullStack.Common.Api;

using System;

namespace HB.FullStack.XamarinForms.Api
{
    public class ApiEventArgs : EventArgs
    {
        public ApiRequestType RequestType { get; set; }

        public string RequestId { get; set; }

        public ApiEventArgs(ApiRequestType requestType, ApiRequest apiRequest)
        {
            RequestType = requestType;
            RequestId = apiRequest.RequestId;
        }

    }
}
