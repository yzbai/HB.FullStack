using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 除内容外构建Request需要的信息
    /// </summary>
    public abstract class HttpRequestBuilder
    {
        public HttpMethodName HttpMethod { get; protected set; }

        public bool NeedHttpMethodOverride { get; protected set; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public ApiAuthType ApiAuthType { get; protected set; }

        public string? ApiKeyName { get; set; }

        protected HttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, ApiAuthType apiAuthType, string? apiKeyName = null)
        {
            HttpMethod = httpMethod;
            NeedHttpMethodOverride = needHttpMethodOveride;
            ApiAuthType = apiAuthType;
            ApiKeyName = apiKeyName;
        }

        public void SetJwt(string jwt)
        {
            Headers[ApiHeaderNames.Authorization] = "Bearer " + jwt;
        }

        public void SetApiKey(string apiKey)
        {
            Headers[ApiHeaderNames.XApiKey] = apiKey;
        }

        public void SetDeviceId(string deviceId)
        {
            Headers[ApiHeaderNames.DEVICE_ID] = deviceId;
        }

        public void SetDeviceVersion(string deviceVersion)
        {
            Headers[ApiHeaderNames.DEVICE_VERSION] = deviceVersion;
        }

        public string GetUrl()
        {
            string uri = GetUrlCore();

            IDictionary<string, string?> parameters = new Dictionary<string, string?>
            {
                { ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) },
                { ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture)}
            };

            return UriUtil.AddQuerys(uri, parameters);
        }

        protected abstract string GetUrlCore();

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(NeedHttpMethodOverride);
            hashCode.Add(ApiAuthType);
            hashCode.Add(ApiKeyName);
            hashCode.Add(HttpMethod);

            foreach (KeyValuePair<string, string> kv in Headers)
            {
                hashCode.Add(kv.Key);
                hashCode.Add(kv.Value);
            }

            return hashCode.ToHashCode();
        }
    }

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

    /// <summary>
    /// 强调Url的组建方式Restful Api方式
    /// </summary>
    public class RestfulHttpRequestBuilder : HttpRequestBuilder
    {
        public string? EndpointName { get; set; }

        public string? ApiVersion { get; set; }

        public string? Condition { get; set; }

        public string? ResName { get; set; }

        public Guid? ResId { get; set; }

        public IList<(string parentResName, string parentResId)> Parents { get; } = new List<(string parentResName, string parentResId)>();

        public void AddParent(string parentResName, string parentResId)
        {
            Parents.Add((parentResName, parentResId));
        }

        protected sealed override string GetUrlCore()
        {
            string? parentSegment = GetParentSegment(Parents);

            if (parentSegment == null && ResId == null)
            {
                return $"{ApiVersion}/{ResName}/{Condition}";
            }
            else if (parentSegment == null && ResId != null)
            {
                return $"{ApiVersion}/{ResName}/{ResId}/{Condition}";
            }
            else if (parentSegment != null && ResId == null)
            {
                return $"{ApiVersion}/{parentSegment}/{ResName}/{Condition}";
            }
            else //if(parentSegment != null && ResId != null)
            {
                return $"{ApiVersion}/{parentSegment}/{ResName}/{ResId}/{Condition}";
            }

            static string? GetParentSegment(IList<(string parentResName, string parentResId)> lst)
            {
                if (lst.IsNullOrEmpty())
                {
                    return null;
                }

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var (parentResName, parentResId) in lst)
                {
                    stringBuilder.Append(parentResName);
                    stringBuilder.Append('/');
                    stringBuilder.Append(parentResId);
                    stringBuilder.Append('/');
                }

                return stringBuilder.RemoveLast().ToString();
            }
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(base.GetHashCode());
            hashCode.Add(EndpointName);
            hashCode.Add(ApiVersion);
            hashCode.Add(Condition);
            hashCode.Add(ResName);
            hashCode.Add(ResId);

            foreach ((string parentResName, string parentResId) in Parents)
            {
                hashCode.Add(parentResId);
                hashCode.Add(parentResName);
            }

            return hashCode.ToHashCode();
        }

        public RestfulHttpRequestBuilder(
            HttpMethodName httpMethod,
            bool needHttpMethodOveride,
            ApiAuthType apiAuthType,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition) : base(httpMethod, needHttpMethodOveride, apiAuthType)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }

        public RestfulHttpRequestBuilder(
            HttpMethodName httpMethod,
            bool needHttpMethodOveride,
            string apiKeyName,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition) : base(httpMethod, needHttpMethodOveride, ApiAuthType.ApiKey, apiKeyName)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }
    }

    public class RestfulHttpRequestBuilder<T> : RestfulHttpRequestBuilder where T : ApiResource2
    {
        public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, ApiAuthType apiAuthType, string? condition) 
            : base(httpMethod, needHttpMethodOveride, apiAuthType, null, null, null, condition)
        {
            SetByApiResourceDef();
        }

        public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, string apiKeyName, string? condition) 
            : base(httpMethod, needHttpMethodOveride, apiKeyName, null, null, null, condition)
        {
            SetByApiResourceDef();
        }

        private void SetByApiResourceDef()
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            EndpointName = def.EndpointName;
            ApiVersion = def.ApiVersion;
            ResName = def.ResName;
        }
    }

    public class RestfulHttpRequestBuilder<TParent, T> : RestfulHttpRequestBuilder where T : ApiResource2 where TParent : ApiResource2
    {
        public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, ApiAuthType apiAuthType, Guid parentId, string? condition)
            : base(httpMethod, needHttpMethodOveride, apiAuthType, null, null, null, condition)
        {
            SetByApiResourceDef(parentId);
        }

        public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, string apiKeyName, Guid parentId, string? condition)
            : base(httpMethod, needHttpMethodOveride, apiKeyName, null, null, null, condition)
        {
            SetByApiResourceDef(parentId);
        }

        private void SetByApiResourceDef(Guid parentId)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            EndpointName = def.EndpointName;
            ApiVersion = def.ApiVersion;
            ResName = def.ResName;

            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            AddParent(paretnDef.ResName, parentId.ToString());
        }
    }

    public class RestfulHttpRequestBuilder<TParent1, TParent2, T> : RestfulHttpRequestBuilder where T : ApiResource2 where TParent1 : ApiResource2 where TParent2 : ApiResource2
    {
        public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, ApiAuthType apiAuthType, Guid parent1Id, Guid parent2Id, string? condition)
            : base(httpMethod, needHttpMethodOveride, apiAuthType, null, null, null, condition)
        {
            SetByApiResourceDef(parent1Id, parent2Id);
        }

        public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, string apiKeyName, Guid parent1Id, Guid parent2Id, string? condition)
            : base(httpMethod, needHttpMethodOveride, apiKeyName, null, null, null, condition)
        {
            SetByApiResourceDef(parent1Id, parent2Id);
        }

        private void SetByApiResourceDef(Guid parent1Id, Guid parent2Id)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            EndpointName = def.EndpointName;
            ApiVersion = def.ApiVersion;
            ResName = def.ResName;

            ApiResourceDef parent1Def = ApiResourceDefFactory.Get<TParent1>();
            ApiResourceDef parent2Def = ApiResourceDefFactory.Get<TParent2>();

            AddParent(parent1Def.ResName, parent1Id.ToString());
            AddParent(parent2Def.ResName, parent2Id.ToString());
        }
    }
}