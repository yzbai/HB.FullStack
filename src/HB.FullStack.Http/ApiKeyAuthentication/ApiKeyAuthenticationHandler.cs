using HB.FullStack.Common.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HB.FullStack.Server.ApiKeyAuthentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyOptions>
    {
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string? providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!Options.TryGetApiKey(providedApiKey, out string? apiKeyName))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key provided."));
            }


            List<Claim> claims = new List<Claim> { new Claim(ClaimExtensionTypes.ApiKeyName, apiKeyName) };

            ClaimsIdentity identity = new ClaimsIdentity(claims, Options.Scheme);
            List<ClaimsIdentity> identities = new List<ClaimsIdentity> { identity };
            ClaimsPrincipal principal = new ClaimsPrincipal(identities);
            AuthenticationTicket ticket = new AuthenticationTicket(principal, Options.Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));

        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = "application/problem+json";

            ApiError errorResponse = new ApiError(ApiErrorCode.NoAuthority);

            return Response.WriteAsync(SerializeUtil.ToJson(errorResponse));
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            Response.ContentType = "application/problem+json";

            ApiError errorResponse = new ApiError(ApiErrorCode.NoAuthority);

            return Response.WriteAsync(SerializeUtil.ToJson(errorResponse));
        }
    }
}
