using HB.Framework.Common.Api;
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

namespace HB.Framework.Client.ApiClient
{
    public class ApiClient : IApiClient
    {
        //move to settings
        private const string _refreshTokenFrequencyCheckResource = "_Fqc_Refresh";

        private static readonly SemaphoreSlim _tokenRefreshSemaphore = new SemaphoreSlim(1, 1);

        private readonly ILogger _logger;

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IClientGlobal _mobileGlobal;

        private readonly MemoryFrequencyChecker _frequencyChecker = new MemoryFrequencyChecker();

        private readonly IDictionary<string, bool> _lastRefreshTokenResults = new Dictionary<string, bool>();

        public ApiClient(IOptions<ApiClientOptions> options, ILogger<ApiClient> logger, IClientGlobal mobileGlobal, IHttpClientFactory httpClientFactory)
        {
            _options = options.ThrowIfNull(nameof(options)).Value;
            _logger = logger;
            _mobileGlobal = mobileGlobal.ThrowIfNull(nameof(mobileGlobal));
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<T>> GetAsync<T>(ApiRequest request) where T : ApiData
        {
            ApiResponse apiResponse = await GetAsync(request, typeof(T));
            ApiResponse<T> typedResponse = new ApiResponse<T>(apiResponse.HttpCode, apiResponse.Message, apiResponse.ErrCode);

            if (apiResponse.Data != null)
            {
                typedResponse.Data = (T)apiResponse.Data;
            }
            return typedResponse;
        }

        public Task<ApiResponse> GetAsync(ApiRequest request)
        {
            return GetAsync(request, null);
        }

        //多次尝试，自动refresh token，
        private async Task<ApiResponse> GetAsync(ApiRequest request, Type dataType)
        {
            ThrowIf.Null(request, nameof(request));

            await AddDeviceIdAlwaysAsync(request).ConfigureAwait(false);

            if (!request.IsValid())
            {
                return new RequestNotValidResponse(request);
            }

            if (!await AddAuthenticateIfNeededAsync(request).ConfigureAwait(false))
            {
                return new NotLoginResponse();
            }

            EndpointSettings endpoint = _options.Endpoints.Single(e => e.ProductType == request.GetProductType() && e.Version == request.GetApiVersion());

            ApiResponse response = await GetResponseCore(request, endpoint, dataType).ConfigureAwait(false);

            return await AutoRefreshTokenAsync(request, response, endpoint, dataType).ConfigureAwait(false);
        }

        #region Privates

        private async Task<ApiResponse> GetResponseCore(ApiRequest request, EndpointSettings endpointSettings, Type dataType)
        {
            using HttpRequestMessage httpRequest = ConstructureHttpRequest(request, endpointSettings);
            HttpClient httpClient = GetHttpClient(endpointSettings);

            using HttpResponseMessage httpResponse = await GetResponseActual(httpRequest, httpClient).ConfigureAwait(false);
            return await ConstructureHttpResponseAsync(httpResponse, dataType).ConfigureAwait(false);
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

        private async Task<ApiResponse> AutoRefreshTokenAsync(ApiRequest request, ApiResponse response, EndpointSettings endpointSettings, Type dataType)
        {
            if (response?.HttpCode != 401 || response?.ErrCode != ApiError.API_TOKEN_EXPIRED || !request.GetNeedAuthenticate())
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
                if (!_frequencyChecker.Check(_refreshTokenFrequencyCheckResource, accessTokenHashKey, TimeSpan.FromSeconds(endpointSettings.TokenRefresh.TokenRefreshIntervalSeconds)))
                {
                    if (_lastRefreshTokenResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult) && lastRefreshResult)
                    {
                        //刷新成功，再次调用
                        return await GetAsync(request, dataType).ConfigureAwait(false);
                    }

                    return response;
                }

                //开始刷新
                string refreshToken = await _mobileGlobal.GetRefreshTokenAsync().ConfigureAwait(false);

                if (!refreshToken.IsNullOrEmpty())
                {
                    ApiRequest refreshRequest = new ApiRequest(
                        endpointSettings.TokenRefresh.TokenRefreshProductType,
                        endpointSettings.TokenRefresh.TokenRefreshVersion,
                        HttpMethod.Put,
                        false,
                        endpointSettings.TokenRefresh.TokenRefreshResourceName);

                    refreshRequest.AddParameter(ClientNames.AccessToken, accessToken);
                    refreshRequest.AddParameter(ClientNames.RefreshToken, refreshToken);

                    EndpointSettings tokenRefreshEndpoint = _options.Endpoints.Single(e => e.ProductType == endpointSettings.TokenRefresh.TokenRefreshProductType && e.Version == endpointSettings.TokenRefresh.TokenRefreshVersion);
                    HttpClient httpClient = GetHttpClient(tokenRefreshEndpoint);

                    using HttpRequestMessage httpRefreshRequest = ConstructureHttpRequest(refreshRequest, endpointSettings);
                    using HttpResponseMessage refreshResponse = await GetResponseActual(httpRefreshRequest, httpClient).ConfigureAwait(false);
                    if (refreshResponse.StatusCode == HttpStatusCode.OK)
                    {
                        _lastRefreshTokenResults[accessTokenHashKey] = true;

                        string jsonString = await refreshResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                        string newAccessToken = SerializeUtil.FromJson(jsonString, ClientNames.AccessToken);

                        await _mobileGlobal.SetAccessTokenAsync(newAccessToken).ConfigureAwait(false);

                        return await GetAsync(request, dataType).ConfigureAwait(false);
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
                string query = request.GetParameters().ToHttpValueCollection().ToString();

                if (!query.IsNullOrEmpty())
                {
                    requestUrlBuilder.Append("?");
                    requestUrlBuilder.Append(query);
                }
            }

            return requestUrlBuilder.ToString();
        }

        private async Task<ApiResponse> ConstructureHttpResponseAsync(HttpResponseMessage httpResponse, Type dataType)
        {
            ThrowIf.Null(httpResponse, nameof(httpResponse));

            //TODO: Using httpResponse.Content.ReadAsStreamAsync() instead.

            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (httpResponse.IsSuccessStatusCode)
            {
                object data = dataType == null ? null : SerializeUtil.FromJson(dataType, content);

                return new ApiResponse(data, (int)httpResponse.StatusCode);
            }
            else
            {
                string mediaType = httpResponse.Content.Headers.ContentType.MediaType;

                if (mediaType == "application/problem+json" || mediaType == "application/json")
                {
                    ApiErrorResponse errorResponse = SerializeUtil.FromJson<ApiErrorResponse>(content);

                    return new ApiResponse((int)httpResponse.StatusCode, errorResponse.Message, errorResponse.Code);
                }
                else
                {
                    return new ApiResponse((int)httpResponse.StatusCode, "Internal Server Error.", ApiError.API_INTERNAL_ERROR);
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
            request.AddParameter(ClientNames.DeviceId, await _mobileGlobal.GetDeviceIdAsync().ConfigureAwait(false));
            //request.AddParameter(MobileInfoNames.DeviceType, await mobileInfoProvider.GetDeviceTypeAsync().ConfigureAwait(false));
            //request.AddParameter(MobileInfoNames.DeviceVersion, await mobileInfoProvider.GetDeviceVersionAsync().ConfigureAwait(false));
            //request.AddParameter(MobileInfoNames.DeviceAddress, await mobileInfoProvider.GetDeviceAddressAsync().ConfigureAwait(false));
        }



        #endregion
    }
}
