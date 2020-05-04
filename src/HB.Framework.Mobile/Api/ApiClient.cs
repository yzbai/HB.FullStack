using HB.Framework.Client.Properties;
using HB.Framework.Common.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Client.Api
{
    public class ApiClient : IApiClient
    {
        //move to settings
        private const string _refreshTokenFrequencyCheckResource = "_Fqc_Refresh";

        private static readonly SemaphoreSlim _tokenRefreshSemaphore = new SemaphoreSlim(1, 1);

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IClientGlobal _mobileGlobal;

        private readonly MemoryFrequencyChecker _frequencyChecker = new MemoryFrequencyChecker();

        private readonly IDictionary<string, bool> _lastRefreshTokenResults = new Dictionary<string, bool>();

        public ApiClient(IOptions<ApiClientOptions> options, IClientGlobal mobileGlobal, IHttpClientFactory httpClientFactory)
        {
            _options = options.ThrowIfNull(nameof(options)).Value;
            _mobileGlobal = mobileGlobal.ThrowIfNull(nameof(mobileGlobal));
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<T>> GetAsync<T>(ApiRequest request) where T : ApiData
        {
            ApiResponse apiResponse = await GetAsync(request, typeof(T)).ConfigureAwait(false);
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
        private async Task<ApiResponse> GetAsync(ApiRequest request, Type? dataType)
        {
            ThrowIf.Null(request, nameof(request));

            await AddDeviceInfoAlwaysAsync(request).ConfigureAwait(false);

            if (!request.IsValid())
            {
                return new RequestNotValidResponse(request);
            }

            if (!await AddAuthenticateIfNeededAsync(request).ConfigureAwait(false))
            {
                return new NotLoginResponse();
            }

            EndpointSettings endpoint = _options.Endpoints.Single(e => e.ProductType == request.GetProductType() && e.Version == request.GetApiVersion());

            ThrowIf.NullOrNotValid(endpoint, nameof(endpoint));

            ApiResponse response = await GetResponseCore(request, endpoint, dataType).ConfigureAwait(false);

            return await AutoRefreshTokenAsync(request, response, endpoint, dataType).ConfigureAwait(false);
        }

        #region Privates

        private async Task<ApiResponse> GetResponseCore(ApiRequest request, EndpointSettings endpointSettings, Type? dataType)
        {
            using HttpRequestMessage httpRequest = ConstructureHttpRequest(request, endpointSettings);
            HttpClient httpClient = GetHttpClient(endpointSettings);

            using HttpResponseMessage httpResponse = await GetResponseActual(httpRequest, httpClient).ConfigureAwait(false);
            return await ConstructureHttpResponseAsync(httpResponse, dataType).ConfigureAwait(false);
        }

        private static Task<HttpResponseMessage> GetResponseActual(HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            return httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
        }

        private async Task<ApiResponse> AutoRefreshTokenAsync(ApiRequest request, ApiResponse response, EndpointSettings endpointSettings, Type? dataType)
        {
            if (response.HttpCode != 401 || response.ErrCode != ApiError.ApiTokenExpired|| !request.GetNeedAuthenticate())
            {
                return response;
            }

            //只处理token过期这一种情况

            await _tokenRefreshSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                string? accessToken = await _mobileGlobal.GetAccessTokenAsync().ConfigureAwait(false);

                if (accessToken.IsNullOrEmpty())
                {
                    return response;
                }

                string accessTokenHashKey = SecurityUtil.GetHash(accessToken!);

                //不久前刷新过
                if (!_frequencyChecker.Check(_refreshTokenFrequencyCheckResource, accessTokenHashKey, TimeSpan.FromSeconds(endpointSettings.TokenRefreshSettings.TokenRefreshIntervalSeconds)))
                {
                    if (_lastRefreshTokenResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult) && lastRefreshResult)
                    {
                        //刷新成功，再次调用
                        return await GetAsync(request, dataType).ConfigureAwait(false);
                    }

                    return response;
                }

                //开始刷新
                string? refreshToken = await _mobileGlobal.GetRefreshTokenAsync().ConfigureAwait(false);

                if (!refreshToken.IsNullOrEmpty())
                {
                    ApiRequest refreshRequest = new ApiRequest(
                        endpointSettings.TokenRefreshSettings!.TokenRefreshProductType!,
                        endpointSettings.TokenRefreshSettings!.TokenRefreshVersion!,
                        HttpMethod.Put,
                        false,
                        endpointSettings.TokenRefreshSettings!.TokenRefreshResourceName!);

                    refreshRequest.AddParameter(ClientNames.AccessToken, accessToken!);
                    refreshRequest.AddParameter(ClientNames.RefreshToken, refreshToken!);

                    EndpointSettings tokenRefreshEndpoint = _options.Endpoints.Single(e => e.ProductType == endpointSettings.TokenRefreshSettings.TokenRefreshProductType && e.Version == endpointSettings.TokenRefreshSettings.TokenRefreshVersion);
                    HttpClient httpClient = GetHttpClient(tokenRefreshEndpoint);

                    using HttpRequestMessage httpRefreshRequest = ConstructureHttpRequest(refreshRequest, endpointSettings);
                    using HttpResponseMessage refreshResponse = await GetResponseActual(httpRefreshRequest, httpClient).ConfigureAwait(false);
                    if (refreshResponse.StatusCode == HttpStatusCode.OK)
                    {
                        _lastRefreshTokenResults[accessTokenHashKey] = true;

                        string jsonString = await refreshResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                        string? newAccessToken = SerializeUtil.FromJson(jsonString, ClientNames.AccessToken);

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

        private static async Task<ApiResponse> ConstructureHttpResponseAsync(HttpResponseMessage httpResponse, Type? dataType)
        {
            ThrowIf.Null(httpResponse, nameof(httpResponse));

            Stream responseStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (httpResponse.IsSuccessStatusCode)
            {
                object? data = dataType == null ? null : await SerializeUtil.FromJsonAsync(dataType, responseStream).ConfigureAwait(false);

                return new ApiResponse(data, (int)httpResponse.StatusCode);
            }
            else
            {
                string mediaType = httpResponse.Content.Headers.ContentType.MediaType;

                if (mediaType == "application/problem+json" || mediaType == "application/json")
                {
                    ApiErrorResponse errorResponse = await SerializeUtil.FromJsonAsync<ApiErrorResponse>(responseStream).ConfigureAwait(false);

                    return new ApiResponse((int)httpResponse.StatusCode, errorResponse.Message, errorResponse.Code);
                }
                else
                {
                    return new ApiResponse((int)httpResponse.StatusCode, Resources.InternalServerErrorMessage, ApiError.ApiInternalError);
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
                string? accessToken = await _mobileGlobal.GetAccessTokenAsync().ConfigureAwait(false);

                if (accessToken.IsNullOrEmpty())
                {
                    return false;
                }

                request.AddHeader("Authorization", "Bearer " + accessToken);
            }

            return true;
        }

        private async Task AddDeviceInfoAlwaysAsync(ApiRequest request)
        {
            request.AddParameter(ClientNames.DeviceId, await _mobileGlobal.GetDeviceIdAsync().ConfigureAwait(false));
            request.AddParameter(ClientNames.DeviceType, await _mobileGlobal.GetDeviceTypeAsync().ConfigureAwait(false));
            request.AddParameter(ClientNames.DeviceVersion, await _mobileGlobal.GetDeviceVersionAsync().ConfigureAwait(false));
            request.AddParameter(ClientNames.DeviceAddress, await _mobileGlobal.GetDeviceAddressAsync().ConfigureAwait(false));
        }



        #endregion
    }
}
