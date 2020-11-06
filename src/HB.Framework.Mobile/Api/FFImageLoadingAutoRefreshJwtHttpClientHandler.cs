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

        private static async Task AddDeviceInfoAsync(HttpRequestMessage request)
        {
            string deviceId = await ClientGlobal.GetDeviceIdAsync().ConfigureAwait(false);
            string deviceInfos = ClientGlobal.DeviceInfos.ToString();
            string deviceVersion = ClientGlobal.DeviceVersion;

            if (request.Method == HttpMethod.Get)
            {
                UriBuilder uriBuilder = new UriBuilder(request.RequestUri);

                NameValueCollection queries = HttpUtility.ParseQueryString(uriBuilder.Query);
                queries[ClientNames.DeviceId] = deviceId;
                queries[ClientNames.DeviceInfos] = deviceInfos;
                queries[ClientNames.DeviceVersion] = deviceVersion;

                uriBuilder.Query = queries.ToString();

                request.RequestUri = uriBuilder.Uri;
            }

            else if (request.Content is MultipartFormDataContent content)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope //当request dispose的时候，httpcontent也会dispose
                content.Add(new StringContent(deviceId), ClientNames.DeviceId);
                content.Add(new StringContent(deviceInfos), ClientNames.DeviceInfos);
                content.Add(new StringContent(deviceVersion), ClientNames.DeviceVersion);
#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            else if (request.Content is StringContent stringContent)
            {
                try
                {
                    string json = await stringContent.ReadAsStringAsync().ConfigureAwait(false);
                    if (string.IsNullOrEmpty(json))
                    {
                        return;
                    }

                    Dictionary<string, object?>? dict = SerializeUtil.FromJson<Dictionary<string, object?>>(json);

                    if (dict == null)
                    {
                        dict = new Dictionary<string, object?>();
                    }

                    dict[ClientNames.DeviceId] = deviceId;
                    dict[ClientNames.DeviceInfos] = deviceInfos;
                    dict[ClientNames.DeviceVersion] = deviceVersion;

                    json = SerializeUtil.ToJson(dict);

                    request.Content.Dispose();
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {

                }
            }
            else if (request.Content == null)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>
                {
                    [ClientNames.DeviceId] = deviceId,
                    [ClientNames.DeviceInfos] = deviceInfos,
                    [ClientNames.DeviceVersion] = deviceVersion
                };

                string json = SerializeUtil.ToJson(dict);

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
        }
    }
}
