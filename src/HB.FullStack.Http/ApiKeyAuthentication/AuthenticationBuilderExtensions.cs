using HB.FullStack.Server.ApiKeyAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyOptions> options)
        {
            return authenticationBuilder.AddScheme<ApiKeyOptions, ApiKeyAuthenticationHandler>(ApiKeyOptions.DefaultScheme, options);
        }

        public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder authenticationBuilder, IConfiguration configuration)
        {
            return authenticationBuilder.AddScheme<ApiKeyOptions, ApiKeyAuthenticationHandler>(ApiKeyOptions.DefaultScheme, options =>
            {

                configuration.Bind(options);

            });
        }
    }
}
