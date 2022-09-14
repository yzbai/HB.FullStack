using System;
using System.Collections.Concurrent;
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

            EnsureApiRequestAuth();
        }

        private void EnsureApiRequestAuth()
        {
            if (Request.Auth == null)
            {
                Request.Auth = Request.ApiMethod switch
                {
                    ApiMethod.Get => ResEndpoint.DefaultReadAuth,
                    ApiMethod.Add
                        or ApiMethod.Update
                        or ApiMethod.Delete
                        or ApiMethod.UpdateFields
                        or ApiMethod.UpdateRelation => ResEndpoint.DefaultWriteAuth,
                    ApiMethod.None => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }
}