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
        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IClientGlobal _global;

        public ApiClient(IOptions<ApiClientOptions> options, IClientGlobal mobileGlobal, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _global = mobileGlobal;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendAsync(ApiRequest request)
        {
            await SendAsync<object>(request).ConfigureAwait(false);
        }

        public async Task<T?> SendAsync<T>(ApiRequest request) where T : class
        {
            await SetDeviceInfoAlwaysAsync(request).ConfigureAwait(false);

            if (!request.IsValid())
            {
                throw new ApiException(ApiErrorCode.MODELVALIDATIONERROR);
            }

            try
            {
                EndpointSettings endpoint = _options.Endpoints.Single(e => e.ProductName == request.GetProductType() && e.Version == request.GetApiVersion());

                HttpClient httpClient = GetHttpClient(endpoint);

                if (request is JwtApiRequest jwtApiRequest)
                {
                    bool jwtAdded = await TrySetJwt(jwtApiRequest).ConfigureAwait(false);

                    if (!jwtAdded)
                    {
                        throw new ApiException(ApiErrorCode.NOAUTHORITY);
                    }
                }

                if (request is ApiKeyRequest apiKeyRequest)
                {
                    if (!TrySetApiKey(apiKeyRequest))
                    {
                        throw new ApiException(ApiErrorCode.NOAUTHORITY);
                    }
                }

                ApiResponse<T> response = await request.GetResponseAsync<T>(httpClient).ConfigureAwait(false);

                if (request is JwtApiRequest)
                {
                    //只处理token过期这一种情况
                    if (response.HttpCode == 401 && response.ErrCode == ApiErrorCode.ApiTokenExpired)
                    {
                        bool refreshSuccessed = await RefreshJwtAsync(endpoint).ConfigureAwait(false);

                        if (refreshSuccessed)
                        {
                            return await SendAsync<T>(request).ConfigureAwait(false);
                        }
                    }
                }

                if (!response.IsSuccessful)
                {
                    throw new ApiException(response.ErrCode, response.Message, response.HttpCode);
                }

                return response.Data;
            }
            catch (ApiException)
            {
                throw;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                throw new ApiException(ex, ApiErrorCode.UnKownError, $"ApiClient.SendAsync Failed.Type : {typeof(T)}");
            }
        }

        private HttpClient GetHttpClient(EndpointSettings endpoint)
        {
            return _httpClientFactory.CreateClient(EndpointSettings.GetHttpClientName(endpoint));
        }

        private async Task<bool> TrySetJwt(JwtApiRequest request)
        {
            string? accessToken = await _global.GetAccessTokenAsync().ConfigureAwait(false);

            if (accessToken.IsNullOrEmpty())
            {
                return false;
            }

            request.SetJwt(accessToken!);

            return true;
        }

        private bool TrySetApiKey(ApiKeyRequest apiKeyRequest)
        {
            if (_options.TryGetApiKey(apiKeyRequest.GetApiKeyName(), out string key))
            {
                apiKeyRequest.SetApiKey(key);
                return true;
            }

            return false;
        }

        private async Task SetDeviceInfoAlwaysAsync(ApiRequest request)
        {
            request.DeviceId = await _global.GetDeviceIdAsync().ConfigureAwait(false);
            request.DeviceType = await _global.GetDeviceTypeAsync().ConfigureAwait(false);
            request.DeviceVersion = await _global.GetDeviceVersionAsync().ConfigureAwait(false);
            //request.DeviceAddress = await _mobileGlobal.GetDeviceAddressAsync().ConfigureAwait(false);
        }

        #region Refresh

        private const string _refreshTokenFrequencyCheckResourceType = "_Fqc_Refresh";

        private static readonly SemaphoreSlim _tokenRefreshSemaphore = new SemaphoreSlim(1, 1);

        private static readonly MemoryFrequencyChecker _frequencyChecker = new MemoryFrequencyChecker();//TODO: 考虑内存占用

        private static readonly IDictionary<string, bool> _lastRefreshTokenResults = new Dictionary<string, bool>();

        private class InnerUpdateTokenResponseData
        {
            public string AccessToken { get; set; } = null!;
        }

        public async Task<bool> RefreshJwtAsync(EndpointSettings endpointSettings)
        {
            try
            {
                //上锁
                await _tokenRefreshSemaphore.WaitAsync().ConfigureAwait(false);

                string? accessToken = await _global.GetAccessTokenAsync().ConfigureAwait(false);

                if (accessToken.IsNullOrEmpty())
                {
                    return false;
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
                        return true;
                    }

                    return false;
                }

                string? refreshToken = await _global.GetRefreshTokenAsync().ConfigureAwait(false);

                if (!refreshToken.IsNullOrEmpty())
                {
                    //开始刷新
                    ApiRequest refreshRequest = new ApiRequest(
                        endpointSettings.JwtSettings!.ProductName!,
                        endpointSettings.JwtSettings!.Version!,
                        HttpMethod.Put,
                        endpointSettings.JwtSettings!.ResourceName!);

                    await SetDeviceInfoAlwaysAsync(refreshRequest).ConfigureAwait(false);

                    refreshRequest.SetParameter(ClientNames.AccessToken, accessToken!);
                    refreshRequest.SetParameter(ClientNames.RefreshToken, refreshToken!);

                    EndpointSettings tokenRefreshEndpoint = _options.Endpoints.Single(
                        e => e.ProductName == endpointSettings.JwtSettings.ProductName &&
                        e.Version == endpointSettings.JwtSettings.Version);

                    HttpClient httpClient = GetHttpClient(tokenRefreshEndpoint);

                    ApiResponse<InnerUpdateTokenResponseData> refreshResponse = await refreshRequest.GetResponseAsync<InnerUpdateTokenResponseData>(httpClient).ConfigureAwait(false);

                    //刷新成功
                    if (refreshResponse.IsSuccessful)
                    {
                        _lastRefreshTokenResults[accessTokenHashKey] = true;

                        string newAccessToken = refreshResponse.Data!.AccessToken;

                        await OnJwtRefreshSucceed(newAccessToken).ConfigureAwait(false);

                        return true;
                    }
                }

                //刷新失败
                _lastRefreshTokenResults[accessTokenHashKey] = false;

                await OnJwtRefreshFailed().ConfigureAwait(false);

                return false;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex, ApiErrorCode.TokenRefresherError, "ApiClient.AutoRefreshTokenAsync Error.");
            }
            finally
            {
                _tokenRefreshSemaphore.Release();
            }
        }

        private async Task OnJwtRefreshSucceed(string? newAccessToken)
        {
            await _global.SetAccessTokenAsync(newAccessToken).ConfigureAwait(false);
        }

        private async Task OnJwtRefreshFailed()
        {
            await _global.SetAccessTokenAsync(null).ConfigureAwait(false);
            await _global.SetRefreshTokenAsync(null).ConfigureAwait(false);
        }

        #endregion
    }
}
