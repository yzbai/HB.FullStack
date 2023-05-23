using System;

namespace HB.FullStack.Client.ApiClient
{
    public class ApiEventArgs : EventArgs
    {
        public long RequestId { get; }
        public ApiMethod RequestHttpMethod { get; }

        public ApiEventArgs(long requestId, ApiMethod requestHttpMethod)
        {
            RequestId = requestId;
            RequestHttpMethod = requestHttpMethod;
        }
    }
}