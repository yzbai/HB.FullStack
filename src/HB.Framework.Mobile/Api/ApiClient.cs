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

        //TODO: 考虑内存占用
        private static readonly MemoryFrequencyChecker _frequencyChecker = new MemoryFrequencyChecker();

        private static readonly IDictionary<string, bool> _lastRefreshTokenResults = new Dictionary<string, bool>();

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IClientGlobal _mobileGlobal;

        public ApiClient(IOptions<ApiClientOptions> options, IClientGlobal mobileGlobal, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _mobileGlobal = mobileGlobal;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse> RequestAsync(ApiRequest request)
        {
            return await RequestAsync<ApiResponseData>(request).ConfigureAwait(false);
        }

        //多次尝试，自动refresh token，
        public async Task<ApiResponse<T>> RequestAsync<T>(ApiRequest request) where T : ApiResponseData
        {
            await AddDeviceInfoAlwaysAsync(request).ConfigureAwait(false);

            if (!request.IsValid())
            {
                return new RequestNotValidResponse(request);
            }

            try
            {
                EndpointSettings endpoint = _options.Endpoints.Single(e => e.ProductName == request.GetProductType() && e.Version == request.GetApiVersion());

                HttpClient httpClient = GetHttpClient(endpoint);

                if (request is JwtApiRequest jwtApiRequest)
                {
                    bool jwtAdded = await TryAddJwt(jwtApiRequest).ConfigureAwait(false);

                    if (!jwtAdded)
                    {
                        return new NotLoginResponse();
                    }

                    ApiResponse<T> response = await jwtApiRequest.GetResponseAsync<T>(httpClient).ConfigureAwait(false);

                    return await AutoRefreshJwtAsync<T>(jwtApiRequest, response, endpoint).ConfigureAwait(false);
                }
                else if (request is ApiKeyApiRequest apiKeyRequest)
                {
                    if (!TryAddApiKey(apiKeyRequest))
                    {
                        return new NotLoginResponse();
                    }

                    return await apiKeyRequest.GetResponseAsync<T>(httpClient).ConfigureAwait(false);
                }
                else
                {
                    return await request.GetResponseAsync<T>(httpClient).ConfigureAwait(false);
                }
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

        private async Task<ApiResponse<T>> AutoRefreshJwtAsync<T>(JwtApiRequest request, ApiResponse response, EndpointSettings endpointSettings) where T : ApiResponseData
        {
            if (response.HttpCode != 401 || response.ErrCode != ApiError.ApiTokenExpired)
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
                    TimeSpan.FromSeconds(endpointSettings.JwtSettings.RefreshIntervalSeconds)))
                {
                    if (_lastRefreshTokenResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult) && lastRefreshResult)
                    {
                        //刷新成功，再次调用
                        return await RequestAsync<T>(request).ConfigureAwait(false);
                    }

                    return response;
                }

                //开始刷新
                string? refreshToken = await _mobileGlobal.GetRefreshTokenAsync().ConfigureAwait(false);

                if (!refreshToken.IsNullOrEmpty())
                {
                    ApiRequest refreshRequest = new ApiRequest(
                        endpointSettings.JwtSettings!.ProductName!,
                        endpointSettings.JwtSettings!.Version!,
                        HttpMethod.Put,
                        endpointSettings.JwtSettings!.ResourceName!);

                    refreshRequest.SetParameter(ClientNames.AccessToken, accessToken!);
                    refreshRequest.SetParameter(ClientNames.RefreshToken, refreshToken!);

                    EndpointSettings tokenRefreshEndpoint = _options.Endpoints.Single(
                        e => e.ProductName == endpointSettings.JwtSettings.ProductName &&
                        e.Version == endpointSettings.JwtSettings.Version);

                    HttpClient httpClient = GetHttpClient(tokenRefreshEndpoint);

                    ApiResponse refreshResponse = await refreshRequest.GetResponseAsync(httpClient).ConfigureAwait(false);

                    //using HttpRequestMessage httpRefreshRequest = refreshRequest.ToHttpRequestMessage(tokenRefreshEndpoint.NeedHttpMethodOveride);
                    //using HttpResponseMessage refreshResponse = await httpRefreshRequest.GetHttpResponseMessage(httpClient).ConfigureAwait(false);

                    if (refreshResponse.IsSuccessful())
                    {
                        _lastRefreshTokenResults[accessTokenHashKey] = true;

                        string newAccessToken = refreshResponse.Data!.GetType().GetProperty(ClientNames.AccessToken).GetValue(refreshResponse.Data).ToString();

                        await OnJwtRefreshSucceed(newAccessToken).ConfigureAwait(false);

                        return await RequestAsync<T>(request).ConfigureAwait(false);
                    }
                }

                //刷新失败
                //frequencyChecker.Reset(frequencyCheckResourceName, accessTokenHash);
                _lastRefreshTokenResults[accessTokenHashKey] = false;

                await OnJwtRefreshFailed().ConfigureAwait(false);

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

        private async Task OnJwtRefreshSucceed(string? newAccessToken)
        {
            await _mobileGlobal.SetAccessTokenAsync(newAccessToken).ConfigureAwait(false);
        }

        private async Task OnJwtRefreshFailed()
        {
            await _mobileGlobal.SetAccessTokenAsync(null).ConfigureAwait(false);
            await _mobileGlobal.SetRefreshTokenAsync(null).ConfigureAwait(false);
        }

        private HttpClient GetHttpClient(EndpointSettings endpoint)
        {
            return _httpClientFactory.CreateClient(EndpointSettings.GetHttpClientName(endpoint));
        }

        private async Task<bool> TryAddJwt(JwtApiRequest request)
        {
            string? accessToken = await _mobileGlobal.GetAccessTokenAsync().ConfigureAwait(false);

            if (accessToken.IsNullOrEmpty())
            {
                return false;
            }

            request.SetJwt(accessToken!);

            return true;
        }

        private bool TryAddApiKey(ApiKeyApiRequest apiKeyRequest)
        {
            if (_options.TryGetApiKey(apiKeyRequest.GetApiKeyName(), out string key))
            {
                apiKeyRequest.SetApiKey(key);
                return true;
            }

            return false;
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
