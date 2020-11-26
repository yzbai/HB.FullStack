using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiKeyRequest : ApiRequest
    {
        public ApiKeyRequest(string productType, string apiVersion, HttpMethod httpMethod, string resourceName, string? condition = null)
            : base(productType, apiVersion, httpMethod, resourceName, condition)
        {
        }

        public void SetApiKey(string apiKey)
        {
            SetHeader("X-Api-Key", apiKey);
        }

        public abstract string GetApiKeyName();
    }
}