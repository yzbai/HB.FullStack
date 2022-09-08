using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class HttpRequestMessageBuilder
    {
        internal ResEndpoint ResEndpoint { get; }

        internal ApiRequest Request { get; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public SiteSetting SiteSetting => ResEndpoint.SiteSetting!;

        public HttpRequestMessageBuilder(ResEndpoint resEndpoint, ApiRequest request)
        {
            ResEndpoint = resEndpoint;
            Request = request;

            EnsureApiRequestAuth(resEndpoint);
        }

        private void EnsureApiRequestAuth(ResEndpoint resEndpoint)
        {
            if (Request.Auth == null)
            {
                Request.Auth = Request.ApiMethod switch
                {
                    ApiMethod.Get => resEndpoint.DefaultReadAuth,
                    ApiMethod.Add => resEndpoint.DefaultWriteAuth,
                    ApiMethod.Update => resEndpoint.DefaultWriteAuth,
                    ApiMethod.Delete => resEndpoint.DefaultWriteAuth,
                    ApiMethod.UpdateFields => resEndpoint.DefaultWriteAuth,
                    ApiMethod.UpdateRelation => resEndpoint.DefaultWriteAuth,
                    ApiMethod.None => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }
}