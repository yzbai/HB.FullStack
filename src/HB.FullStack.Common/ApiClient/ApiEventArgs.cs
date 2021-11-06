using HB.FullStack.Common.Api;

using System;
using System.Net.Http;

namespace HB.FullStack.Common.ApiClient
{
    public class ApiEventArgs : EventArgs
    {
        public string RequestId { get; }
        public HttpMethod RequestHttpMethod { get;}

        public ApiEventArgs(string requestId, HttpMethod requestHttpMethod)
        {
            RequestId = requestId;
            RequestHttpMethod = requestHttpMethod;
        }

    }
}
