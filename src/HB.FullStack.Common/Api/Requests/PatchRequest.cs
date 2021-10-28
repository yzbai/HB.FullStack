using System;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 更新几个字段
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PatchRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        protected PatchRequest(string condition) : base(HttpMethod.Patch, condition)
        {

        }

        protected PatchRequest(string condition, string apiKeyName) : base(apiKeyName, HttpMethod.Patch, condition)
        {

        }

        public override string ToDebugInfo()
        {
            return $"PatchRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(this)}";
        }
    }
}