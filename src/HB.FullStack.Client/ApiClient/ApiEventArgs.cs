using System;

namespace HB.FullStack.Client.ApiClient
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
