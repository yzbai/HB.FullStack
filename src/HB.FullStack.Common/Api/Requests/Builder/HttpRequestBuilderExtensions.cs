using System;
using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public static class HttpRequestBuilderExtensions
    {
        private static readonly Version _version20 = new Version(2, 0);

        /// <summary>
        /// 构建HTTP的基本信息
        /// </summary>
        public static HttpRequestMessage Build(this HttpRequestBuilder builder)
        {
            HttpMethod httpMethod = builder.HttpMethod.ToHttpMethod();

            if (builder.NeedHttpMethodOverride && (httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Delete))
            {
                builder.Headers["X-HTTP-Method-Override"] = httpMethod.Method;
                httpMethod = HttpMethod.Post;
            }

            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, builder.GetUrl())
            {
                Version = _version20
            };

            foreach (var kv in builder.Headers)
            {
                httpRequest.Headers.Add(kv.Key, kv.Value);
            }

            return httpRequest;
        }
    }

    //public class RestfulHttpRequestBuilder<TParent, T> : RestfulHttpRequestBuilder where T : ApiResource2 where TParent : ApiResource2
    //{
    //    public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, ApiAuthType apiAuthType, Guid parentId, string? condition)
    //        : base(httpMethod, needHttpMethodOveride, apiAuthType, null, null, null, condition)
    //    {
    //        SetByApiResourceDef(parentId);
    //    }

    //    public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, string apiKeyName, Guid parentId, string? condition)
    //        : base(httpMethod, needHttpMethodOveride, apiKeyName, null, null, null, condition)
    //    {
    //        SetByApiResourceDef(parentId);
    //    }

    //    private void SetByApiResourceDef(Guid parentId)
    //    {
    //        ApiResourceDef def = ApiResourceDefFactory.Get<T>();

    //        EndpointName = def.EndpointName;
    //        ApiVersion = def.Version;
    //        ResName = def.ResName;

    //        ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

    //        AddParent(paretnDef.ResName, parentId.ToString());
    //    }
    //}

    //public class RestfulHttpRequestBuilder<TParent1, TParent2, T> : RestfulHttpRequestBuilder where T : ApiResource2 where TParent1 : ApiResource2 where TParent2 : ApiResource2
    //{
    //    public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, ApiAuthType apiAuthType, Guid parent1Id, Guid parent2Id, string? condition)
    //        : base(httpMethod, needHttpMethodOveride, apiAuthType, null, null, null, condition)
    //    {
    //        SetByApiResourceDef(parent1Id, parent2Id);
    //    }

    //    public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, string apiKeyName, Guid parent1Id, Guid parent2Id, string? condition)
    //        : base(httpMethod, needHttpMethodOveride, apiKeyName, null, null, null, condition)
    //    {
    //        SetByApiResourceDef(parent1Id, parent2Id);
    //    }

    //    private void SetByApiResourceDef(Guid parent1Id, Guid parent2Id)
    //    {
    //        ApiResourceDef def = ApiResourceDefFactory.Get<T>();

    //        EndpointName = def.EndpointName;
    //        ApiVersion = def.Version;
    //        ResName = def.ResName;

    //        ApiResourceDef parent1Def = ApiResourceDefFactory.Get<TParent1>();
    //        ApiResourceDef parent2Def = ApiResourceDefFactory.Get<TParent2>();

    //        AddParent(parent1Def.ResName, parent1Id.ToString());
    //        AddParent(parent2Def.ResName, parent2Id.ToString());
    //    }
    //}
}