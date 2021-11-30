using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 更新几个字段
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UpdateFieldsRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        protected UpdateFieldsRequest(string? condition) : base(HttpMethodName.Patch, condition) { }

        protected UpdateFieldsRequest(string apiKeyName, string? condition) : base(apiKeyName, HttpMethodName.Patch, condition) { }

        public override string ToDebugInfo()
        {
            return $"PatchRequest, ApiResourceType:{typeof(T).Name}, Json:{SerializeUtil.ToJson(this)}";
        }
    }

    public abstract class UpdateFieldsRequest2<T, TOwner> : UpdateFieldsRequest<T> where T : ApiResource2 where TOwner : ApiResource2
    {
        /// <summary>
        /// 主要Resource 的ID
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public string OwnerResName { get; set; } = null!;
        protected UpdateFieldsRequest2(Guid ownerId, string? condition) : base(condition)
        {
            ApiResourceDef ownerDef = ApiResourceDefFactory.Get<TOwner>();
            OwnerId = ownerId;
            OwnerResName = ownerDef.ResName;
        }

        public override string ToDebugInfo()
        {
            return $"UpdateFieldsRequest, ApiResourceType:{typeof(T).Name}, OwnerResourceType:{typeof(TOwner).Name},  Json:{SerializeUtil.ToJson(this)}";
        }

        protected override string GetUrlCore()
        {
            return $"{ApiVersion}/{OwnerResName}/{OwnerId}/{ResName}/{Condition}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), OwnerId, OwnerResName);
        }
    }
}