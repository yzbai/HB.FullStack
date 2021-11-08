using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using HB.FullStack.Common.Utility;
using HB.FullStack.Common.Api;
using System.Collections.Generic;
using System.Globalization;

namespace HB.FullStack.Common.ApiClient
{
    public class TokenAutoRefreshedHttpClientHandler : HttpClientHandler
    {
        private readonly IApiClient _apiClient;
        private readonly IApiTokenProvider _tokenProvider;
        private readonly ApiClientOptions _options;

        public TokenAutoRefreshedHttpClientHandler(IApiClient apiClient, IApiTokenProvider tokenProvider, IOptions<ApiClientOptions> options)
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

        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            EndpointSettings? endpointSettings = GetEndpointByUri(request.RequestUri);

            if (endpointSettings == null)
            {
                GlobalSettings.Logger.LogDebug("Not found endpoint");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            AddTokenInfo(request, _tokenProvider);

            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                await HttpClientApiExtensions.ThrowIfNotSuccessedAsync(responseMessage).ConfigureAwait(false);
            }
            catch (ErrorCode2Exception ex)
            {
                if (ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    await TokenRefresher.RefreshAccessTokenAsync(_apiClient, endpointSettings, _tokenProvider).ConfigureAwait(false);

                    return responseMessage;
                }

                GlobalSettings.Logger.Log(LogLevel.Critical, ex, "FFImageLoading的权限认证图片挂掉了！");
            }

            return responseMessage;
        }

        private static void AddTokenInfo(HttpRequestMessage request, IApiTokenProvider tokenProvider)
        {
            //Jwt
            if (tokenProvider.AccessToken.IsNotNullOrEmpty())
            {
                request.Headers.Add("Authorization", "Bearer " + tokenProvider.AccessToken);
            }

            //Url
            IDictionary<string, string?> parameters = new Dictionary<string, string?>
            {
                { ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) },
                { ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture)},
                
                //额外添加DeviceId，为了验证jwt中的DeviceId与本次请求deviceiId一致
                { ClientNames.DEVICE_ID, tokenProvider.DeviceId }
            };

            request.RequestUri = new Uri(UriUtil.AddQuerys(request.RequestUri.ToString(), parameters));
        }

        private EndpointSettings? GetEndpointByUri(Uri? requestUri)
        {
            string authority = requestUri!.Authority;

            return _options.Endpoints.FirstOrDefault(endpoint =>
            {
                return authority.StartsWith(endpoint.Url!.Authority, StringComparison.InvariantCultureIgnoreCase);
            });
        }
    }
}
