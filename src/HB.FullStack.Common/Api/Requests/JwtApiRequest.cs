using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public class JwtApiRequest<T> : ApiRequest<T> where T : Resource
    {
        public JwtApiRequest(HttpMethod httpMethod, string? condition)
            : base(httpMethod, condition)
        {
        }

        public void SetJwt(string jwt)
        {
            SetHeader("Authorization", "Bearer " + jwt);
        }
    }
}