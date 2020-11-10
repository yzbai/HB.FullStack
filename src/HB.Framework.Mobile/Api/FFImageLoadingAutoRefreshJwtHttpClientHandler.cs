using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using HB.Framework.Common.Api;
using Microsoft.Extensions.Options;
using Xamarin.Forms;
using Microsoft.Extensions.Logging;
using HB.Framework.Common.Utility;

namespace HB.Framework.Client.Api
{
    public class FFImageLoadingAutoRefreshJwtHttpClientHandler : HttpClientHandler
    {
        private readonly IApiClient _apiClient;
        private readonly ApiClientOptions _options;

        public FFImageLoadingAutoRefreshJwtHttpClientHandler(IApiClient apiClient, IOptions<ApiClientOptions> options)
        {
            _apiClient = apiClient;
            _options = options.Value;

#if DEBUG
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost", GlobalSettings.Comparison))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#endif
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AddDeviceInfoAsync(request).ConfigureAwait(false);
            await AddAuthorizationAsync(request).ConfigureAwait(false);

            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                ApiResponse response = await responseMessage.ToApiResponseAsync().ConfigureAwait(false);

                if (response.HttpCode == 401 && response.ErrCode == ErrorCode.ApiTokenExpired)
                {
                    EndpointSettings? endpointSettings = GetEndpointByUri(request.RequestUri);

                    if (endpointSettings != null)
                    {
                        await _apiClient.RefreshJwtAsync(endpointSettings).ConfigureAwait(false);
                    }
                }

                //刷新后，等待下次自动Retry
                return responseMessage;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Application.Current.Log(Microsoft.Extensions.Logging.LogLevel.Critical, ex, "FFImageLoading的权限认证图片挂掉了！");
                return responseMessage;
            }
        }

        private EndpointSettings? GetEndpointByUri(Uri requestUri)
        {
            string authority = requestUri.Authority;

            return _options.Endpoints.FirstOrDefault(endpoint =>
            {
                return authority.StartsWith(endpoint.Url!.Authority, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private static async Task AddAuthorizationAsync(HttpRequestMessage request)
        {
            string? token = await ClientGlobal.GetAccessTokenAsync().ConfigureAwait(false);

            request.Headers.Add("Authorization", "Bearer " + token);
        }

        private class DeviceWrapper
        {
            public string DeviceId { get; set; } = null!;
            public string DeviceVersion { get; set; } = null!;
            public DeviceInfos DeviceInfos { get; set; } = null!;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:丢失范围之前释放对象", Justification = "<挂起>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:不捕获常规异常类型", Justification = "<挂起>")]
        private static async Task AddDeviceInfoAsync(HttpRequestMessage request)
        {
            string deviceId = await ClientGlobal.GetDeviceIdAsync().ConfigureAwait(false);

            DeviceWrapper deviceWrapper = new DeviceWrapper
            {
                DeviceId = deviceId,
                DeviceVersion = ClientGlobal.DeviceVersion,
                DeviceInfos = ClientGlobal.DeviceInfos
            };

            // 因为Jwt要验证DeviceId与token中的是否一致，所以在url的query中加上DeviceId
            request.RequestUri = request.RequestUri.AddQuery(ClientNames.DeviceId, deviceId);
            
            StringContent deviceContent = new StringContent(SerializeUtil.ToJson(deviceWrapper), Encoding.UTF8, "application/json");

            if (request.Content == null)
            {
                request.Content = deviceContent;
            }
            else if (request.Content is MultipartFormDataContent content)
            {
                content.Add(deviceContent);
            }
            else if (request.Content is StringContent stringContent)
            {
                try
                {
                    MultipartContent multipartContent = new MultipartContent();

                    multipartContent.Add(request.Content);
                    multipartContent.Add(deviceContent);
               
                    request.Content = multipartContent;
                }
                catch (Exception ex)
                {
                    Application.Current.Log(LogLevel.Error, ex, $"Url:{request.RequestUri.AbsoluteUri}");
                }
            }
        }
    }
}
