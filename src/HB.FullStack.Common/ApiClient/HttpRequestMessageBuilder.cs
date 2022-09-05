using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class HttpRequestMessageBuilder
    {
        internal ResEndpoint ResBinding { get; }
        internal ApiRequest Request { get; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public SiteSetting EndpointSetting => ResBinding.SiteSetting!;

        public HttpRequestMessageBuilder(ResEndpoint resBinding, ApiRequest request)
        {
            ResBinding = resBinding;
            Request = request;

            EnsureApiRequestAuth(resBinding);
        }

        private void EnsureApiRequestAuth(ResEndpoint resBinding)
        {
            if (Request.Auth == null)
            {
                Request.Auth = Request.ApiMethod switch
                {
                    ApiMethod.Get => resBinding.ReadAuth,
                    ApiMethod.Add => resBinding.WriteAuth,
                    ApiMethod.Update => resBinding.WriteAuth,
                    ApiMethod.Delete => resBinding.WriteAuth,
                    ApiMethod.UpdateFields => resBinding.WriteAuth,
                    ApiMethod.None => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }
}