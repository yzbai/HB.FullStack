using Microsoft.AspNetCore.Authentication;

namespace HB.Framework.Authentication.ApiKey
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "_HB_Framework_API_KEY";
        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;
    }
}