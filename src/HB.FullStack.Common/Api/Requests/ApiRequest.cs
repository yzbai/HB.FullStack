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
        /// NOTICE: JsonIgnore避免Server端收到。RequestBuilder只对构建Request有用。
        /// 一个Request包含两部分：1，业务内容（各种业务数据信息）；2，http构建信息
        /// http构建信息放到RequestBuilder，不被服务端接收
        /// </summary>
        [JsonIgnore]
        public HttpRequestMessageBuilder? RequestBuilder { get; }

        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        [OnlyForJsonConstructor]
        protected ApiRequest() { }

        protected ApiRequest(HttpRequestMessageBuilder requestBuilder)
        {
            RequestBuilder = requestBuilder;
        }

        /// <summary>
        /// NOTICE: 排除了RequestId，所以相同条件的Request的HashCode相同
        /// </summary>
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            if (RequestBuilder != null)
            {
                hashCode.Add(RequestBuilder);
            }

            return hashCode.ToHashCode();
        }
    }
}