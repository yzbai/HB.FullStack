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

namespace HB.FullStack.Client.Api
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
            AddDeviceInfo(request);
            await AddAuthInfoAsync(request).ConfigureAwait(false);

            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                await HttpClientApiExtensions.ThrowIfNotSuccessedAsync(responseMessage).ConfigureAwait(false);
            }
            catch (ApiException ex)
            {
                if (ex.HttpCode == System.Net.HttpStatusCode.Unauthorized && ex.ErrorCode == ErrorCode.ApiTokenExpired)
                {
                    EndpointSettings? endpointSettings = GetEndpointByUri(request.RequestUri);

                    if (endpointSettings != null)
                    {
                        await TokenRefresher.RefreshAccessTokenAsync(_apiClient, endpointSettings).ConfigureAwait(false);

                        return responseMessage;
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                Application.Current.Log(LogLevel.Critical, ex, "FFImageLoading的权限认证图片挂掉了！");

            }

            return responseMessage;
        }

        private EndpointSettings? GetEndpointByUri(Uri requestUri)
        {
            string authority = requestUri.Authority;

            return _options.Endpoints.FirstOrDefault(endpoint =>
            {
                return authority.StartsWith(endpoint.Url!.Authority, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private static async Task AddAuthInfoAsync(HttpRequestMessage request)
        {
            string? token = await UserPreferences.GetAccessTokenAsync().ConfigureAwait(false);

            request.Headers.Add("Authorization", "Bearer " + token);
        }

        private static void AddDeviceInfo(HttpRequestMessage request)
        {
            string deviceId = DevicePreferences.GetDeviceId();

            // 因为Jwt要验证DeviceId与token中的是否一致，所以在url的query中加上DeviceId
            request.RequestUri = request.RequestUri.AddQuery(ClientNames.DeviceId, deviceId);

            DeviceWrapper deviceWrapper = new DeviceWrapper
            {
                DeviceId = deviceId,
                DeviceVersion = DevicePreferences.DeviceVersion,
                DeviceInfos = DevicePreferences.DeviceInfos
            };

#pragma warning disable CA2000 // Dispose objects before losing scope
            StringContent deviceContent = new StringContent(SerializeUtil.ToJson(deviceWrapper), Encoding.UTF8, "application/json");
#pragma warning restore CA2000 // Dispose objects before losing scope

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

        private class DeviceWrapper
        {
            public string DeviceId { get; set; } = null!;
            public string DeviceVersion { get; set; } = null!;
            public DeviceInfos DeviceInfos { get; set; } = null!;
        }
    }
}
