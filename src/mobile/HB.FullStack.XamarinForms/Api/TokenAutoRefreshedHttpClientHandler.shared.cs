using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using Xamarin.Forms;
using Microsoft.Extensions.Logging;
using HB.FullStack.Common.Utility;
using HB.FullStack.Common.Api;
using System.Collections.Generic;

namespace HB.FullStack.XamarinForms.Api
{
    public class TokenAutoRefreshedHttpClientHandler : HttpClientHandler
    {
        private readonly IApiClient _apiClient;
        private readonly ApiClientOptions _options;

        public TokenAutoRefreshedHttpClientHandler(IApiClient apiClient, IOptions<ApiClientOptions> options)
        {
            _apiClient = apiClient;
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
        /// <exception cref="ApiException">Ignore.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            EndpointSettings? endpointSettings = GetEndpointByUri(request.RequestUri);

            if (endpointSettings == null)
            {
                GlobalSettings.Logger.LogDebug("Not found endpoint");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            AddDeviceInfo(request);
            AddAuthInfo(request);

            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                await HttpClientApiExtensions.ThrowIfNotSuccessedAsync(responseMessage).ConfigureAwait(false);
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    await TokenRefresher.RefreshAccessTokenAsync(_apiClient, endpointSettings).ConfigureAwait(false);

                    return responseMessage;
                }

                GlobalSettings.Logger.Log(LogLevel.Critical, ex, "FFImageLoading的权限认证图片挂掉了！");
            }

            return responseMessage;
        }

        private EndpointSettings? GetEndpointByUri(Uri? requestUri)
        {
            string authority = requestUri!.Authority;

            return _options.Endpoints.FirstOrDefault(endpoint =>
            {
                return authority.StartsWith(endpoint.Url!.Authority, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private static void AddAuthInfo(HttpRequestMessage request)
        {
            if (UserPreferences.AccessToken.IsNotNullOrEmpty())
            {
                request.Headers.Add("Authorization", "Bearer " + UserPreferences.AccessToken);
            }
        }

        private static void AddDeviceInfo(HttpRequestMessage request)
        {
            string deviceId = DevicePreferences.DeviceId;

            // 因为Jwt要验证DeviceId与token中的是否一致，所以在url的query中加上DeviceId

            request.RequestUri = new Uri( UrlUtil.AddQuerys(request.RequestUri.ToString(), new Dictionary<string, string?> { { ClientNames.DeviceId, deviceId } }));

            //DeviceWrapper deviceWrapper = new()
            //{
            //    DeviceId = deviceId,
            //    DeviceVersion = DevicePreferences.DeviceVersion,
            //    DeviceInfos = DevicePreferences.DeviceInfos
            //};

            //StringContent deviceContent = new StringContent(SerializeUtil.ToJson(deviceWrapper), Encoding.UTF8, "application/json");

            //if (request.Content == null)
            //{
            //    request.Content = deviceContent;
            //}
            //else if (request.Content is MultipartFormDataContent content)
            //{
            //    content.Add(deviceContent);
            //}
            //else if (request.Content is StringContent stringContent)
            //{
            //    try
            //    {
            //        MultipartContent multipartContent = new()
            //        {
            //            request.Content,
            //            deviceContent
            //        };

            //        request.Content = multipartContent;
            //    }
            //    catch (Exception ex)
            //    {
            //        GlobalSettings.Logger.Log(LogLevel.Error, ex, $"Url:{request.RequestUri.AbsoluteUri}");
            //    }
            //}
        }

        private class DeviceWrapper
        {
            public string DeviceId { get; set; } = null!;
            public string DeviceVersion { get; set; } = null!;
            public DeviceInfos DeviceInfos { get; set; } = null!;
        }
    }
}
