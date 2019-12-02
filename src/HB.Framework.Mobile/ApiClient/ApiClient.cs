using HB.Framework.Common.Mobile;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Mobile.ApiClient
{
    public class ApiClient : IApiClient
    {
        //move to settings
        private const string _refreshTokenFrequencyCheckResource = "_Fqc_Refresh";

        private static readonly SemaphoreSlim _tokenRefreshSemaphore = new SemaphoreSlim(1, 1);

        private readonly ILogger _logger;

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IMobileGlobal _mobileGlobal;

        private readonly MemoryFrequencyChecker _frequencyChecker = new MemoryFrequencyChecker();

        private readonly IDictionary<string, bool> _lastRefreshTokenResults = new Dictionary<string, bool>();

        public ApiClient(IOptions<ApiClientOptions> options, ILogger<ApiClient> logger, IMobileGlobal mobileGlobal, IHttpClientFactory httpClientFactory)
        {
            _options = options.ThrowIfNull(nameof(options)).Value;
            _logger = logger;
            _mobileGlobal = mobileGlobal.ThrowIfNull(nameof(mobileGlobal));
            _httpClientFactory = httpClientFactory;
        }

        //多次尝试，自动refresh token，
        public async Task<ApiResponse<T>> GetAsync<T>(ApiRequest request) where T : ApiData
        {
            ThrowIf.Null(request, nameof(request));

            await AddDeviceIdAlwaysAsync(request).ConfigureAwait(false);

            if (!request.IsValid())
            {
                return new RequestNotValidResponse<T>(request);
            }

            if (!await AddAuthenticateIfNeededAsync(request).ConfigureAwait(false))
            {
                return new NotLoginResponse<T>();
            }

            EndpointSettings endpoint = _options.Endpoints.Single(e => e.ProductType == request.GetProductType() && e.Version == request.GetApiVersion());

            ApiResponse<T> response = await GetResponseCore<T>(request, endpoint).ConfigureAwait(false);

            return await AutoRefreshTokenAsync(request, response, endpoint).ConfigureAwait(false);
        }

        #region Privates

        private async Task<ApiResponse<T>> GetResponseCore<T>(ApiRequest request, EndpointSettings endpointSettings) where T : ApiData
        {
            using HttpRequestMessage httpRequest = ConstructureHttpRequest(request, endpointSettings);
            HttpClient httpClient = GetHttpClient(endpointSettings);

            using HttpResponseMessage httpResponse = await GetResponseActual(httpRequest, httpClient).ConfigureAwait(false);
            return await ConstructureHttpResponseAsync<T>(httpResponse).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> GetResponseActual(HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            TraceLog(httpRequestMessage, httpResponseMessage);

            return httpResponseMessage;
        }

        private void TraceLog(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse)
        {
            _logger.LogTrace($"Request {httpRequest.RequestUri}, Response {httpResponse.StatusCode}");
        }

        private async Task<ApiResponse<T>> AutoRefreshTokenAsync<T>(ApiRequest request, ApiResponse<T> response, EndpointSettings endpointSettings) where T : ApiData
        {
            if (response?.HttpCode != 401 || response?.ErrCode != ErrorCode.API_TOKEN_EXPIRED || !request.GetNeedAuthenticate())
            {
                return response;
            }

            //只处理token过期这一种情况

            await _tokenRefreshSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                string accessToken = await _mobileGlobal.GetAccessTokenAsync().ConfigureAwait(false);

                if (accessToken.IsNullOrEmpty())
                {
                    return response;
                }

                string accessTokenHashKey = SecurityUtil.GetHash(accessToken);

                //不久前刷新过
                if (!_frequencyChecker.Check(_refreshTokenFrequencyCheckResource, accessTokenHashKey, TimeSpan.FromSeconds(endpointSettings.TokenRefreshIntervalSeconds)))
                {
                    if (_lastRefreshTokenResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult) && lastRefreshResult)
                    {
                        //刷新成功，再次调用
                        return await GetAsync<T>(request).ConfigureAwait(false);
                    }

                    return response;
                }

                //开始刷新
                string refreshToken = await _mobileGlobal.GetRefreshTokenAsync().ConfigureAwait(false);

                if (!refreshToken.IsNullOrEmpty())
                {
                    ApiRequest refreshRequest = new ApiRequest(
                        endpointSettings.TokenRefreshProductType,
                        endpointSettings.TokenRefreshVersion,
                        HttpMethod.Put,
                        false,
                        endpointSettings.TokenRefreshResourceName);

                    refreshRequest.AddParameter(MobileInfoNames.AccessToken, accessToken);
                    refreshRequest.AddParameter(MobileInfoNames.RefreshToken, refreshToken);

                    EndpointSettings tokenRefreshEndpoint = _options.Endpoints.Single(e => e.ProductType == endpointSettings.TokenRefreshProductType && e.Version == endpointSettings.TokenRefreshVersion);
                    HttpClient httpClient = GetHttpClient(tokenRefreshEndpoint);

                    using HttpRequestMessage httpRefreshRequest = ConstructureHttpRequest(refreshRequest, endpointSettings);
                    using HttpResponseMessage refreshResponse = await GetResponseActual(httpRefreshRequest, httpClient).ConfigureAwait(false);
                    if (refreshResponse.StatusCode == HttpStatusCode.OK)
                    {
                        _lastRefreshTokenResults[accessTokenHashKey] = true;

                        string jsonString = await refreshResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                        string newAccessToken = SerializeUtil.FromJson(jsonString, MobileInfoNames.AccessToken);

                        await _mobileGlobal.SetAccessTokenAsync(newAccessToken).ConfigureAwait(false);

                        return await GetAsync<T>(request).ConfigureAwait(false);
                    }
                }

                //刷新失败
                //frequencyChecker.Reset(frequencyCheckResourceName, accessTokenHash);
                _lastRefreshTokenResults[accessTokenHashKey] = false;

                await _mobileGlobal.SetAccessTokenAsync(null).ConfigureAwait(false);
                await _mobileGlobal.SetRefreshTokenAsync(null).ConfigureAwait(false);


                return response;
            }
            finally
            {
                _tokenRefreshSemaphore.Release();
            }
        }

        private static HttpRequestMessage ConstructureHttpRequest(ApiRequest request, EndpointSettings endpointSettings)
        {
            HttpMethod httpMethod = request.GetHttpMethod();

            if (endpointSettings.NeedHttpMethodOveride && (httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Delete))
            {
                request.AddHeader("X-HTTP-Method-Override", httpMethod.Method);
                httpMethod = HttpMethod.Post;
            }

            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, ConstructureRequestUrl(request));

            if (request.GetHttpMethod() != HttpMethod.Get)
            {
                httpRequest.Content = new FormUrlEncodedContent(request.GetParameters());
            }

            request.GetHeaders().ForEach(kv => httpRequest.Headers.Add(kv.Key, kv.Value));

            return httpRequest;
        }

        private static string ConstructureRequestUrl(ApiRequest request)
        {
            StringBuilder requestUrlBuilder = new StringBuilder();

            if (!request.GetApiVersion().IsNullOrEmpty())
            {
                requestUrlBuilder.Append(request.GetApiVersion());
            }

            if (!request.GetResourceName().IsNullOrEmpty())
            {
                requestUrlBuilder.Append("/");
                requestUrlBuilder.Append(request.GetResourceName());
            }

            if (!request.GetCondition().IsNullOrEmpty())
            {
                requestUrlBuilder.Append("/");
                requestUrlBuilder.Append(request.GetCondition());
            }

            if (request.GetHttpMethod() == HttpMethod.Get)
            {
                //string query = request.GetParameters().Select(kv => {
                //    return new KeyValuePair<string, string>(kv.Key, HttpUtility.UrlEncode(kv.Value));
                //}).ToHttpValueCollection().ToString();

                string query = request.GetParameters().ToHttpValueCollection().ToString();

                if (!query.IsNullOrEmpty())
                {
                    requestUrlBuilder.Append("?");
                    requestUrlBuilder.Append(query);
                }
            }

            return requestUrlBuilder.ToString();
        }

        private async Task<ApiResponse<T>> ConstructureHttpResponseAsync<T>(HttpResponseMessage httpResponse) where T : ApiData
        {
            ThrowIf.Null(httpResponse, nameof(httpResponse));

            //TODO: Using httpResponse.Content.ReadAsStreamAsync() instead.

            string con 处理Api的Ok（）返回。没有实体   tent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (httpResponse.IsSuccessStatusCode)
            {
                T resource = SerializeUtil.FromJson<T>(content);

                return new ApiResponse<T>(resource, (int)httpResponse.StatusCode);
            }
            else
            {
                string mediaType = httpResponse.Content.Headers.ContentType.MediaType;

                if (mediaType == "application/problem+json" || mediaType == "application/json")
                {
                    ErrorResponse errorResponse = SerializeUtil.FromJson<ErrorResponse>(content);

                    return new ApiResponse<T>((int)httpResponse.StatusCode, errorResponse.Message, errorResponse.Code);
                }
                else
                {
                    return new ApiResponse<T>((int)httpResponse.StatusCode, "Internal Server Error.", ErrorCode.API_INTERNAL_ERROR);
                }
            }
        }

        private HttpClient GetHttpClient(EndpointSettings endpoint)
        {
            return _httpClientFactory.CreateClient(EndpointSettings.GetHttpClientName(endpoint));
        }

        private async Task<bool> AddAuthenticateIfNeededAsync(ApiRequest request)
        {
            if (request.GetNeedAuthenticate())
            {
                string accessToken = await _mobileGlobal.GetAccessTokenAsync().ConfigureAwait(false);

                if (accessToken.IsNullOrEmpty())
                {
                    return false;
                }

                request.AddHeader("Authorization", "Bearer " + accessToken);
            }

            return true;
        }

        private async Task AddDeviceIdAlwaysAsync(ApiRequest request)
        {
            request.AddParameter(MobileInfoNames.DeviceId, await _mobileGlobal.GetDeviceIdAsync().ConfigureAwait(false));
            //request.AddParameter(MobileInfoNames.DeviceType, await mobileInfoProvider.GetDeviceTypeAsync().ConfigureAwait(false));
            //request.AddParameter(MobileInfoNames.DeviceVersion, await mobileInfoProvider.GetDeviceVersionAsync().ConfigureAwait(false));
            //request.AddParameter(MobileInfoNames.DeviceAddress, await mobileInfoProvider.GetDeviceAddressAsync().ConfigureAwait(false));
        }

        #endregion
    }
}
