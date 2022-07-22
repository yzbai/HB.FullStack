global using OnlyForJsonConstructorAttribute = System.Text.Json.Serialization.JsonConstructorAttribute;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiRequest : ValidatableObject, IDTO
    {
        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        [Required]
        public string ResName { get; set; } = null!;

        //[JsonIgnore]
        public ApiMethodName ApiMethodName { get; set; }

        //[JsonIgnore]
        public ApiRequestAuth Auth { get; set; }

        //[JsonIgnore]
        public string? Condition { get; set; }

        [OnlyForJsonConstructor]
        protected ApiRequest() { }

        protected ApiRequest(string resName, ApiMethodName apiMethodName, ApiRequestAuth auth, string? condition)
        {
            ResName = resName;
            ApiMethodName = apiMethodName;
            Auth = auth;
            Condition = condition;
        }

        /// <summary>
        /// NOTICE: 排除了RequestId，所以相同条件的Request的HashCode相同
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(ApiMethodName, Auth, Condition, ResName);
        }
    }

    /// <summary>
    /// 指明是对哪一个资源的请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //public abstract class ApiRequest<T> : ApiRequest where T : ApiResource
    //{
    //    [OnlyForJsonConstructor]
    //    protected ApiRequest() { }

    //    protected ApiRequest(ApiMethodName apiMethodName, ApiRequestAuth auth, string? condition)
    //        : base(apiMethodName, auth, typeof(T).Name, condition) { }

    //    //protected sealed override HttpRequestBuilder CreateHttpRequestBuilder()
    //    //{
    //    //    return CreateRestfulHttpRequestBuilder(this);
    //    //}

    //}
}