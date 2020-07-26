using HB.Framework.Client.Properties;
using HB.Framework.Common.Api;
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
        private const string _refreshTokenFrequencyCheckResourceType = "_Fqc_Refresh";

        private static readonly SemaphoreSlim _tokenRefreshSemaphore = new SemaphoreSlim(1, 1);

        private readonly MemoryFrequencyChecker _frequencyChecker = new MemoryFrequencyChecker();

        private readonly IDictionary<string, bool> _lastRefreshTokenResults = new Dictionary<string, bool>();

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IClientGlobal _mobileGlobal;

        public ApiClient(IOptions<ApiClientOptions> options, IClientGlobal mobileGlobal, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _mobileGlobal = mobileGlobal;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<T>> RequestAsync<T>(ApiRequest request) where T : ApiResponseData
        {
            ApiResponse apiResponse = await RequestAsync(request, typeof(T)).ConfigureAwait(false);
            ApiResponse<T> typedResponse = new ApiResponse<T>(apiResponse.HttpCode, apiResponse.Message, apiResponse.ErrCode);

            if (apiResponse.Data != null)
            {
                typedResponse.Data = apiResponse.Data as T;
            }

            return typedResponse;
        }

        public Task<ApiResponse> RequestAsync(ApiRequest request)
        {
            return RequestAsync(request, null);
        }

        //多次尝试，自动refresh token，
        private async Task<ApiResponse> RequestAsync(ApiRequest request, Type? dataType)
        {
            await AddDeviceInfoAlwaysAsync(request).ConfigureAwait(false);

            if (!request.IsValid())
            {
                return new RequestNotValidResponse(request);
            }

            if (!await AddAuthenticateIfNeededAsync(request).ConfigureAwait(false))
            {
                return new NotLoginResponse();
            }

            try
            {
                EndpointSettings endpoint = _options.Endpoints.Single(e => e.ProductType == request.GetProductType() && e.Version == request.GetApiVersion());

                HttpClient httpClient = GetHttpClient(endpoint);

                ApiResponse response = await request.GetApiResponse(dataType, httpClient, endpoint.NeedHttpMethodOveride).ConfigureAwait(false);

                return await AutoRefreshTokenAsync(request, response, endpoint, dataType).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                return new EndpointNotFoundResponse();
            }
            catch (FrameworkException)
            {
                return new EndpointNotFoundResponse();
            }
        }

        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        /// <exception cref="SemaphoreFullException">Ignore.</exception>
        /// <exception cref="HB.Framework.Client.ClientException"></exception>
        private async Task<ApiResponse> AutoRefreshTokenAsync(ApiRequest request, ApiResponse response, EndpointSettings endpointSettings, Type? dataType)
        {
            if (response.HttpCode != 401 || response.ErrCode != ApiError.ApiTokenExpired || !request.GetNeedAuthenticate())
            {
                return response;
            }

            try
            {
                //只处理token过期这一种情况

                await _tokenRefreshSemaphore.WaitAsync().ConfigureAwait(false);

                string? accessToken = await _mobileGlobal.GetAccessTokenAsync().ConfigureAwait(false);

                if (accessToken.IsNullOrEmpty())
                {
                    return response;
                }

                string accessTokenHashKey = SecurityUtil.GetHash(accessToken!);

                //不久前刷新过
                if (!_frequencyChecker.Check(
                    _refreshTokenFrequencyCheckResourceType,
                    accessTokenHashKey,
                    TimeSpan.FromSeconds(endpointSettings.TokenRefreshSettings.TokenRefreshIntervalSeconds)))
                {
                    if (_lastRefreshTokenResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult) && lastRefreshResult)
                    {
                        //刷新成功，再次调用
                        return await RequestAsync(request, dataType).ConfigureAwait(false);
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

                    EndpointSettings tokenRefreshEndpoint = _options.Endpoints.Single(
                        e => e.ProductType == endpointSettings.TokenRefreshSettings.TokenRefreshProductType &&
                        e.Version == endpointSettings.TokenRefreshSettings.TokenRefreshVersion);

                    HttpClient httpClient = GetHttpClient(tokenRefreshEndpoint);

                    using HttpRequestMessage httpRefreshRequest = refreshRequest.ToHttpRequestMessage(tokenRefreshEndpoint.NeedHttpMethodOveride);
                    using HttpResponseMessage refreshResponse = await httpRefreshRequest.GetHttpResponseMessage(httpClient).ConfigureAwait(false);

                    if (refreshResponse.StatusCode == HttpStatusCode.OK)
                    {
                        _lastRefreshTokenResults[accessTokenHashKey] = true;

                        string jsonString = await refreshResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                        string? newAccessToken = SerializeUtil.FromJson(jsonString, ClientNames.AccessToken);

                        await _mobileGlobal.SetAccessTokenAsync(newAccessToken).ConfigureAwait(false);

                        return await RequestAsync(request, dataType).ConfigureAwait(false);
                    }
                }

                //刷新失败
                //frequencyChecker.Reset(frequencyCheckResourceName, accessTokenHash);
                _lastRefreshTokenResults[accessTokenHashKey] = false;

                await _mobileGlobal.SetAccessTokenAsync(null).ConfigureAwait(false);
                await _mobileGlobal.SetRefreshTokenAsync(null).ConfigureAwait(false);


                return response;
            }
            catch (Exception ex)
            {
                throw new ClientException("ApiClient.AutoRefreshTokenAsync Error.", ex);
            }
            finally
            {
                _tokenRefreshSemaphore.Release();
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
            request.DeviceId = await _mobileGlobal.GetDeviceIdAsync().ConfigureAwait(false);
            request.DeviceType = await _mobileGlobal.GetDeviceTypeAsync().ConfigureAwait(false);
            request.DeviceVersion = await _mobileGlobal.GetDeviceVersionAsync().ConfigureAwait(false);
            //request.DeviceAddress = await _mobileGlobal.GetDeviceAddressAsync().ConfigureAwait(false);
        }

    }
}
