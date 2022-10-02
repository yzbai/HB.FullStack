using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Common.ApiClient
{
    public class UserTokenRefreshHttpClientHandler : HttpClientHandler
    {
        private readonly IApiClient _apiClient;
        private readonly IPreferenceProvider _tokenProvider;
        private readonly ApiClientOptions _options;

        public UserTokenRefreshHttpClientHandler(IApiClient apiClient, IPreferenceProvider tokenProvider, IOptions<ApiClientOptions> options)
        {
            _apiClient = apiClient;
            _tokenProvider = tokenProvider;
            _options = options.Value;

#if DEBUG
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert!.Issuer.Equals("CN=localhost", GlobalSettings.Comparison))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#endif

            GlobalSettings.Logger.LogInformation("TokenAutoRefreshedHttpClientHandler Inited.");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SiteSetting? endpointSettings = GetEndpointByUri(request.RequestUri);

            if (endpointSettings == null)
            {
                GlobalSettings.Logger.LogDebug("Not found endpoint");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            AddRequestInfo(request, _tokenProvider);

            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                await HttpClientApiExtensions.ThrowIfNotSuccessedAsync(responseMessage, endpointSettings.Challenge).ConfigureAwait(false);
            }
            catch (ErrorCodeException ex)
            {
                if (ex.ErrorCode == ErrorCodes.AccessTokenExpired)
                {
                    await UserTokenRefresher.RefreshUserTokenAsync(_apiClient).ConfigureAwait(false);

                    return responseMessage;
                }

                GlobalSettings.Logger.Log(LogLevel.Critical, ex, "FFImageLoading的权限认证图片挂掉了！");
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
                    .AddQuery(ClientNames.DEVICE_ID, tokenProvider.DeviceId)
                    .AddNoiseQuery()
                    .ToUri();
        }

        private SiteSetting? GetEndpointByUri(Uri? requestUri)
        {
            string authority = requestUri!.Authority;

            return _options.SiteSettings.FirstOrDefault(endpoint =>
            {
                return authority.StartsWith(endpoint.BaseUrl!.Authority, StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}
