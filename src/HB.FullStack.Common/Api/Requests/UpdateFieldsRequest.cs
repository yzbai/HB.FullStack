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
#if NETSTANDARD2_1 || NET5_0_OR_GREATER

        protected UpdateFieldsRequest(string? condition) : base(HttpMethod.Patch, condition) { }

#elif NETSTANDARD2_0
        protected UpdateFieldsRequest(string? condition) : base(new HttpMethod("Patch"), condition) { }
#endif

#if NETSTANDARD2_1 || NET5_0_OR_GREATER

        protected UpdateFieldsRequest(string apiKeyName, string? condition) : base(apiKeyName, HttpMethod.Patch, condition) { }

#elif NETSTANDARD2_0
        protected UpdateFieldsRequest(string apiKeyName, string? condition) : base(apiKeyName, new HttpMethod("Patch"), condition) { }
#endif

        public override string ToDebugInfo()
        {
            return $"PatchRequest, ApiResourceType:{typeof(T).Name}, Json:{SerializeUtil.ToJson(this)}";
        }
    }

    public abstract class UpdateFieldsRequest<T, TSub> : ApiRequest<T, TSub> where T : ApiResource2 where TSub : ApiResource2
    {
#if NETSTANDARD2_1 || NET5_0_OR_GREATER

        protected UpdateFieldsRequest(Guid id, string? condition) : base(id, HttpMethod.Patch, condition) { }

#elif NETSTANDARD2_0
        protected UpdateFieldsRequest(Guid id, string? condition) : base(id, new HttpMethod("Patch"), condition) { }
#endif

#if NETSTANDARD2_1 || NET5_0_OR_GREATER

        protected UpdateFieldsRequest(string apiKeyName, Guid id, string? condition) : base(id, apiKeyName, HttpMethod.Patch, condition) { }

#elif NETSTANDARD2_0
        protected UpdateFieldsRequest(string apiKeyName, Guid id, string? condition) : base(id, apiKeyName, new HttpMethod("Patch"), condition) { }
#endif

        public override string ToDebugInfo()
        {
            return $"UpdateFieldsRequest, ApiResourceType:{typeof(T).Name}, SubResourceType:{typeof(TSub).Name},  Json:{SerializeUtil.ToJson(this)}";
        }
    }
}