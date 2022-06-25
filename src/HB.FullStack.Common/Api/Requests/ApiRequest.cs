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
        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        /// <summary>
        /// 不需要被服务器端看到
        /// </summary>
        [JsonIgnore]
        public HttpMethodName HttpMethodName { get; set; }

        /// <summary>
        /// 不需要被服务器端看到
        /// </summary>
        [JsonIgnore]
        public string? Condition { get; set; }

        [OnlyForJsonConstructor]
        protected ApiRequest() { }

        protected ApiRequest(HttpMethodName httpMethodName, string? condition)
        {
            HttpMethodName = httpMethodName;
            Condition = condition;
        }

        /// <summary>
        /// NOTICE: 排除了RequestId，所以相同条件的Request的HashCode相同
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(HttpMethodName, Condition);
        }
    }
}