using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Client.ApiClient
{
    /// <summary>
    /// 包含构造一个Request的所有信息
    /// </summary>
    public abstract class ApiRequest : ValidatableObject//, IValidatableDTO
    {
        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public long RequestId { get; } = StaticIdGen.GetLongId();

        [JsonIgnore]
        [Required]
        public string ResName { get; set; } = null!;

        [JsonIgnore]
        public ApiMethod ApiMethod { get; set; }

        /// <summary>
        /// 如果没有指定，那么会使用ResBinding中指定的Auth
        /// </summary>
        [JsonIgnore]
        public ApiRequestAuth? Auth { get; set; }

        [JsonIgnore]
        public string? Condition { get; set; }

        /// <summary>
        /// 之后会加到HttpRequestMessageBuilder中去
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        protected ApiRequest(string resName, ApiMethod apiMethod, ApiRequestAuth? auth, string? condition)
        {
            ResName = resName;
            ApiMethod = apiMethod;
            Auth = auth;
            Condition = condition;
        }
    }
}