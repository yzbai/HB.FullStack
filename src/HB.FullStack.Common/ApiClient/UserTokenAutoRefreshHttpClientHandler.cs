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
    public class UserTokenAutoRefreshHttpClientHandler : HttpClientHandler
    {
        private readonly IApiClient _apiClient;
        private readonly IPreferenceProvider _tokenProvider;
        private readonly ApiClientOptions _options;

        public UserTokenAutoRefreshHttpClientHandler(IApiClient apiClient, IPreferenceProvider tokenProvider, IOptions<ApiClientOptions> options)
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
            EndpointSetting? endpointSettings = GetEndpointByUri(request.RequestUri);

            if (endpointSettings == null)
            {
                GlobalSettings.Logger.LogDebug("Not found endpoint");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            AddTokenInfo(request, _tokenProvider);

            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                await HttpClientApiExtensions.ThrowIfNotSuccessedAsync(responseMessage, endpointSettings.Challenge).ConfigureAwait(false);
            }
            catch (ErrorCode2Exception ex)
            {
                if (ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    await UserTokenRefresher.RefreshUserTokenAsync(_apiClient).ConfigureAwait(false);

                    return responseMessage;
                }

                GlobalSettings.Logger.Log(LogLevel.Critical, ex, "FFImageLoading的权限认证图片挂掉了！");
            }

            return responseMessage;
        }

        private static void AddTokenInfo(HttpRequestMessage request, IPreferenceProvider tokenProvider)
        {
            //Jwt
            if (tokenProvider.AccessToken.IsNotNullOrEmpty())
            {
                request.Headers.Add("Authorization", "Bearer " + tokenProvider.AccessToken);
            }

            //BaseUrl
            IDictionary<string, string?> parameters = new Dictionary<string, string?>
            {
                { ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) },
                { ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture)},
                
                //额外添加DeviceId，为了验证jwt中的DeviceId与本次请求deviceiId一致
                { ClientNames.DEVICE_ID, tokenProvider.DeviceId }
            };

            request.RequestUri = new Uri(UriUtil.AddQuerys(request.RequestUri?.ToString(), parameters));
        }

        private EndpointSetting? GetEndpointByUri(Uri? requestUri)
        {
            string authority = requestUri!.Authority;

            return _options.EndpointSettings.FirstOrDefault(endpoint =>
            {
                return authority.StartsWith(endpoint.BaseUrl!.Authority, StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}
