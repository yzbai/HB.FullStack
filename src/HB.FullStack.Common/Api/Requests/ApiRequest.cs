global using OnlyForJsonConstructorAttribute = System.Text.Json.Serialization.JsonConstructorAttribute;

using System;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// ApiRequest包含两部分，一是必须由每个Request决定的构建信息，二是业务数据
    /// </summary>
    public abstract class ApiRequest : ValidatableObject
    {
        #region Builder

        private HttpRequestBuilder? _requestBuilder;

        public HttpRequestBuilder GetHttpRequestBuilder()
        {
            return _requestBuilder ??= CreateHttpRequestBuilder();
        }

        protected abstract HttpRequestBuilder CreateHttpRequestBuilder();

        #endregion

        #region 由每个Request决定的构建信息

        /// <summary>
        /// 不需要被服务器端看到
        /// </summary>
        [JsonIgnore]
        public ApiMethodName ApiMethodName { get; set; }

        /// <summary>
        /// 不需要被服务器端看到
        /// </summary>
        [JsonIgnore]
        public ApiRequestAuth Auth { get; set; }

        /// <summary>
        /// 不需要被服务器端看到
        /// </summary>
        [JsonIgnore]
        public string? Condition { get; set; }

        #endregion

        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        [OnlyForJsonConstructor]
        protected ApiRequest() { }

        protected ApiRequest(ApiMethodName apiMethodName, ApiRequestAuth auth, string? condition)
        {
            ApiMethodName = apiMethodName;
            Auth = auth;
            Condition = condition;
        }

        /// <summary>
        /// NOTICE: 排除了RequestId，所以相同条件的Request的HashCode相同
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(ApiMethodName, Auth, Condition);
        }
    }

    public abstract class ApiRequest<T> : ApiRequest where T : ApiResource2
    {
        [OnlyForJsonConstructor]
        protected ApiRequest() { }

        protected ApiRequest(ApiMethodName apiMethodName, ApiRequestAuth auth, string? condition) : base(apiMethodName, auth, condition) { }

        protected sealed override HttpRequestBuilder CreateHttpRequestBuilder()
        {
            return CreateRestfulHttpRequestBuilder(this);
        }

        /// <summary>
        /// 参数都是由ApiRequest决定的
        /// </summary>
        private static RestfulHttpRequestBuilder CreateRestfulHttpRequestBuilder(ApiRequest<T> apiRequest)
        {
            RestfulHttpRequestBuilder builder = new RestfulHttpRequestBuilder(apiRequest.ApiMethodName, apiRequest.Auth, apiRequest.Condition, null, null, null);

            ApiResourceDef? def = ApiResourceDefFactory.Get<T>();

            if (def == null)
            {
                throw ApiExceptions.LackApiResourceAttribute(typeof(T).FullName);
            }

            //From Res Def
            builder.EndpointName = def.EndpointName;
            builder.ApiVersion = def.Version;
            builder.ResName = def.ResName;

            builder.Parent1ResName = def.Parent1ResName;
            builder.Parent2ResName = def.Parent2ResName;

            builder.Parent1ResId = def.Parent1ResIdGetMethod?.Invoke(apiRequest, null)?.ToString();
            builder.Parent2ResId = def.Parent2ResIdGetMethod?.Invoke(apiRequest, null)?.ToString();

            return builder;
        }
    }
}