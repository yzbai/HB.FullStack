using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 更新几个字段
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UpdateFieldsRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        protected UpdateFieldsRequest(string? condition) : base(HttpMethod.Patch, condition) { }

        protected UpdateFieldsRequest(string apiKeyName, string? condition) : base(apiKeyName, HttpMethod.Patch, condition) { }

        public override string ToDebugInfo()
        {
            return $"PatchRequest, ApiResourceType:{typeof(T).Name}, Json:{SerializeUtil.ToJson(this)}";
        }
    }

    public abstract class UpdateFieldsRequest<T, TSub> : ApiRequest<T, TSub> where T : ApiResource2 where TSub : ApiResource2
    {

        protected UpdateFieldsRequest(Guid id, string? condition) : base(id, HttpMethod.Patch, condition)
        {
        }

        protected UpdateFieldsRequest(string apiKeyName, Guid id, string? condition) : base(id, apiKeyName, HttpMethod.Patch, condition)
        {
        }

        public override string ToDebugInfo()
        {
            return $"UpdateFieldsRequest, ApiResourceType:{typeof(T).Name}, SubResourceType:{typeof(TSub).Name},  Json:{SerializeUtil.ToJson(this)}";
        }
    }
}