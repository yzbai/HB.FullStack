using System;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 强调Url的组件方式是简单的传入。
    /// </summary>
    public class PlainUrlHttpRequestBuilder : HttpRequestBuilder
    {
        public string PlainUrl { get; }

        public PlainUrlHttpRequestBuilder(
            HttpMethodName httpMethod,
            bool needHttpMethodOveride,
            ApiAuthType apiAuthType,
            string plainUrl) : base(httpMethod, needHttpMethodOveride, apiAuthType)
        {
            PlainUrl = plainUrl;
        }

        public PlainUrlHttpRequestBuilder(
            HttpMethodName httpMethod,
            bool needHttpMethodOveride,
            string apiKeyName,
            string plainUrl) : base(httpMethod, needHttpMethodOveride, ApiAuthType.ApiKey, apiKeyName)
        {
            PlainUrl = plainUrl;
        }

        protected override string GetUrlCore()
        {
            return PlainUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), PlainUrl);
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