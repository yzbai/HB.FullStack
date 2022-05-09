

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 只强调数据
    /// </summary>
    public abstract class ApiRequest : ValidatableObject
    {
        /// <summary>
        /// JsonIgnore避免Server端收到。RequestBuilder只对构建Request有用。
        /// </summary>
        [JsonIgnore]
        public HttpRequestBuilder? RequestBuilder { get; }

        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        protected ApiRequest() { }

        protected ApiRequest(HttpRequestBuilder requestBuilder)
        {
            RequestBuilder = requestBuilder;
        }

        /// <summary>
        /// 排除了RequestId，所以相同条件的Request的HashCode相同
        /// </summary>
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            if (RequestBuilder != null)
            {
                hashCode.Add(RequestBuilder.GetHashCode());
            }

            return hashCode.ToHashCode();
        }
    }

    public abstract class ApiRequest<T> : ApiRequest where T : ApiResource2
    {
        [JsonIgnore]
        public new RestfulHttpRequestBuilder? RequestBuilder => (RestfulHttpRequestBuilder?)base.RequestBuilder;

        protected ApiRequest() { }

        protected ApiRequest(RestfulHttpRequestBuilder requestBuildInfo)
        {

        }

        /// <summary>
        /// 因为不会直接使用ApiRequest作为Api的请求参数，所以不用提供无参构造函数，而具体的子类需要提供
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="condition">同一Verb下的条件分支，比如在ApiController上标注的[HttpGet("BySms")],BySms就是condition</param>
        protected ApiRequest(HttpMethodName httpMethod, string? condition) : this(ApiAuthType.Jwt, httpMethod, condition) { }

        protected ApiRequest(string apiKeyName, HttpMethodName httpMethod, string? condition) : this(ApiAuthType.ApiKey, httpMethod, condition)
        {
            if (BuildInfo is RestfulHttpRequestBuilder builder)
            {
                builder.ApiKeyName = apiKeyName;
            }
        }

        protected ApiRequest(ApiAuthType apiAuthType, HttpMethodName httpMethod, string? condition)
            : base(httpMethod, apiAuthType, null, null, null, condition)
        {
            if (BuildInfo is RestfulHttpRequestBuilder builder)
            {
                ApiResourceDef def = ApiResourceDefFactory.Get<T>();

                builder.EndpointName = def.EndpointName;
                builder.ApiVersion = def.ApiVersion;
                builder.ResName = def.ResName;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}