using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public class JwtApiRequest : ApiRequest
    {
        public JwtApiRequest(string productName, string apiVersion, HttpMethod httpMethod, string resourceName, string? condition = null)
            : base(productName, apiVersion, httpMethod, resourceName, condition)
        {
        }

        public void SetJwt(string jwt)
        {
            SetHeader("Authorization", "Bearer " + jwt);
        }
    }
}