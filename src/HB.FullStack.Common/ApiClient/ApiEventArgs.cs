using System;
using System.Net.Http;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Common.ApiClient
{
    public class ApiEventArgs : EventArgs
    {
        public string RequestId { get; }
        public ApiMethod RequestHttpMethod { get; }

        public ApiEventArgs(string requestId, ApiMethod requestHttpMethod)
        {
            RequestId = requestId;
            RequestHttpMethod = requestHttpMethod;
        }

    }
}
