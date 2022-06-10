using HB.FullStack.Common.Api;

using System;
using System.Net.Http;

namespace HB.FullStack.Common.ApiClient
{
    public class ApiEventArgs : EventArgs
    {
        public string RequestId { get; }
        public HttpMethodName RequestHttpMethod { get;}

        public ApiEventArgs(string requestId, HttpMethodName requestHttpMethod)
        {
            RequestId = requestId;
            RequestHttpMethod = requestHttpMethod;
        }

    }
}
