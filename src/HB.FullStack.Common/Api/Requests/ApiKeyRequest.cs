using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiKeyRequest<T> : ApiRequest<T> where T : Resource
    {
        public ApiKeyRequest(HttpMethod httpMethod, string? condition)
            : base(httpMethod, condition)
        {
        }

        public void SetApiKey(string apiKey)
        {
            SetHeader("X-Api-Key", apiKey);
        }

        public abstract string GetApiKeyName();
    }
}