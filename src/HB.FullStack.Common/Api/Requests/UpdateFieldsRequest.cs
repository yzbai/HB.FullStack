using System;
using System.Collections.Generic;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 更新几个字段
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UpdateFieldsRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        protected UpdateFieldsRequest(string condition) : base(HttpMethod.Patch, condition) { }

        protected UpdateFieldsRequest(string apiKeyName, string condition) : base(apiKeyName, HttpMethod.Patch, condition) { }

        public override string ToDebugInfo()
        {
            return $"PatchRequest, ApiResourceType:{typeof(T).Name}, Json:{SerializeUtil.ToJson(this)}";
        }
    }
}