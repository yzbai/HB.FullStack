global using OnlyForJsonConstructorAttribute = System.Text.Json.Serialization.JsonConstructorAttribute;

using System;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 只强调数据
    /// </summary>
    public abstract class ApiRequest : ValidatableObject
    {
        #region Part of Build Info

        /// <summary>
        /// 不需要被服务器端看到
        /// </summary>
        [JsonIgnore]
        public HttpMethodName HttpMethodName { get; set; }

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

        protected ApiRequest(HttpMethodName httpMethodName, ApiRequestAuth auth, string? condition)
        {
            HttpMethodName = httpMethodName;
            Auth = auth;
            Condition = condition;
        }

        /// <summary>
        /// NOTICE: 排除了RequestId，所以相同条件的Request的HashCode相同
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(HttpMethodName, Condition);
        }

        //NOTICE: 如果多于一个参数，可以另设一个endpoint setting class包含这些由应用指定的参数
        protected abstract ApiRequestBuilder CreateBuilder();

        private ApiRequestBuilder? _requestBuilder;
        
        public ApiRequestBuilder GetBuilder()
        {
            return _requestBuilder ??= CreateBuilder();
        }
    }
}