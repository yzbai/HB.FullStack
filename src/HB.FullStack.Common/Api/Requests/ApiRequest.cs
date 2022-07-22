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

        ///// <summary>
        ///// NOTICE: 排除了RequestId，所以相同条件的Request的HashCode相同
        ///// </summary>
        //public sealed override int GetHashCode()
        //{
        //    return HashCode.Combine(GetChildHashCode(), ApiMethodName, Auth, Condition, ResName);
        //}
    }
}