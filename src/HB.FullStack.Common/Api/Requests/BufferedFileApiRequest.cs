using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public abstract class BufferedFileApiRequest<T> : JwtApiRequest<T> where T : Resource
    {
        public BufferedFileApiRequest(HttpMethod httpMethod, string? condition)
            : base(httpMethod, condition)
        {
        }

        public abstract byte[]? GetBytes();

        public abstract string GetBytesPropertyName();

        public abstract string GetFileName();
    }
}