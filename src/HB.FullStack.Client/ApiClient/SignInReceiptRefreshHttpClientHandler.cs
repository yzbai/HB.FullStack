using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.ApiClient
{
    public class SignInReceiptRefreshHttpClientHandler : HttpClientHandler
    {
        private readonly IApiClient _apiClient;
        private readonly IPreferenceProvider _preferenceProvider;
        private readonly ApiClientOptions _options;

        public SignInReceiptRefreshHttpClientHandler(IApiClient apiClient, IPreferenceProvider preferenceProvider, IOptions<ApiClientOptions> options)
        {
            _apiClient = apiClient;
            _preferenceProvider = preferenceProvider;
            _options = options.Value;

#if DEBUG
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert!.Issuer.Equals("CN=localhost", Globals.Comparison))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#endif

            Globals.Logger.LogInformation("TokenAutoRefreshedHttpClientHandler Inited.");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SiteSetting? endpointSettings = GetEndpointByUri(request.RequestUri);

            if (endpointSettings == null)
            {
                Globals.Logger.LogError($"SignInReceiptRefreshHttpClientHandler Not found endpoint for {request.RequestUri}");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            AddRequestInfo(request, _preferenceProvider);

            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                await HttpClientApiExtensions.ThrowIfNotSuccessedAsync(responseMessage, endpointSettings.Challenge).ConfigureAwait(false);
            }
            catch (ErrorCodeException ex)
            {
                if (ex.ErrorCode == ErrorCodes.AccessTokenExpired)
                {
                    await SignInReceiptRefresher.RefreshSignInReceiptAsync(_apiClient, _preferenceProvider, _options.SignInReceiptRefreshIntervalSeconds).ConfigureAwait(false);

                    return responseMessage;
                }

                Globals.Logger.Log(LogLevel.Critical, ex, "FFImageLoading的权限认证图片挂掉了！");
            }

            return responseMessage;
        }

        private static void AddRequestInfo(HttpRequestMessage request, IPreferenceProvider tokenProvider)
        {
            //Headers
            if (tokenProvider.AccessToken.IsNotNullOrEmpty())
            {
                request.Headers.Add("Authorization", "Bearer " + tokenProvider.AccessToken);
            }

            //Query
            request.RequestUri = request.RequestUri?.ToString()
                    .AddQuery(ClientNames.CLIENT_ID, tokenProvider.ClientId)
                    .AddNoiseQuery()
                    .ToUri();
        }

        private SiteSetting? GetEndpointByUri(Uri? requestUri)
        {
            string authority = requestUri!.Authority;

            return _options.OtherSiteSettings.FirstOrDefault(endpoint =>
            {
                return authority.StartsWith(endpoint.BaseUrl!.Authority, StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}
