using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public abstract class BufferedFileApiRequest : JwtApiRequest
    {
        public BufferedFileApiRequest(string productType, string apiVersion, HttpMethod httpMethod, string resourceName, string? condition = null)
            : base(productType, apiVersion, httpMethod, resourceName, condition)
        {
        }

        public abstract byte[]? GetBytes();

        public abstract string GetBytesPropertyName();

        public abstract string GetFileName();
    }
}