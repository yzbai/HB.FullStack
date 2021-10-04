using System;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    public abstract class UpdateFieldsRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        protected UpdateFieldsRequest(string condition) : base(HttpMethod.Put, condition)
        {

        }

        protected UpdateFieldsRequest(string condition, string apiKeyName) : base(apiKeyName, HttpMethod.Put, condition)
        {

        }

        public override string ToDebugInfo()
        {
            return $"UpdateFieldsRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(this)}";
        }
    }
}