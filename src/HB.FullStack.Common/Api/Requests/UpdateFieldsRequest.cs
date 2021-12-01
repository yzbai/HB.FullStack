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
        protected UpdateFieldsRequest(Guid? resId, Guid? ownerResId, string? condition) : base(HttpMethodName.Patch, condition, ownerResId, resId) { }

        protected UpdateFieldsRequest(string apiKeyName, Guid? resId, Guid? ownerResId, string? condition) : base(apiKeyName, HttpMethodName.Patch, condition, ownerResId, resId) { }

        public override string ToDebugInfo()
        {
            return $"PatchRequest, ApiResourceType:{typeof(T).Name}, Json:{SerializeUtil.ToJson(this)}";
        }
    }
}