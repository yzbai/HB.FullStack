using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class HttpRequestMessageBuilder
    {
        internal ResBinding ResBinding { get; }
        internal ApiRequest Request { get; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public EndpointSetting EndpointSetting => ResBinding.EndpointSetting!;

        public HttpRequestMessageBuilder(ResBinding resBinding, ApiRequest request)
        {
            ResBinding = resBinding;
            Request = request;

            EnsureApiRequestAuth(resBinding);
        }

        private void EnsureApiRequestAuth(ResBinding resBinding)
        {
            if (Request.Auth == null)
            {
                Request.Auth = Request.ApiMethodName switch
                {
                    ApiMethodName.Get => resBinding.ReadAuth,
                    ApiMethodName.Post => resBinding.WriteAuth,
                    ApiMethodName.Put => resBinding.WriteAuth,
                    ApiMethodName.Delete => resBinding.WriteAuth,
                    ApiMethodName.Patch => resBinding.WriteAuth,
                    ApiMethodName.None => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }
}