using HB.Framework.Common.Api;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Client.Api
{
    public class ApiClient : IApiClient
    {
        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClient(IOptions<ApiClientOptions> options, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendAsync(ApiRequest request)
        {
            await SetDeviceInfoAlwaysAsync(request).ConfigureAwait(false);

            if (!request.IsValid())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError);
            }

            try
            {
                EndpointSettings endpoint = _options.Endpoints.Single(e => e.ProductName == request.GetProductName() && e.Version == request.GetApiVersion());

                HttpClient httpClient = GetHttpClient(endpoint);

                if (request is JwtApiRequest jwtApiRequest)
                {
                    bool jwtAdded = await TrySetJwtAsync(jwtApiRequest).ConfigureAwait(false);

                    if (!jwtAdded)
                    {
                        throw new ApiException(ErrorCode.ApiNoAuthority);
                    }
                }

                if (request is ApiKeyRequest apiKeyRequest)
                {
                    if (!TrySetApiKey(apiKeyRequest))
                    {
                        throw new ApiException(ErrorCode.ApiNoAuthority);
                    }
                }

                ApiResponse response = await request.GetResponseAsync(httpClient).ConfigureAwait(false);

                if (request is JwtApiRequest)
                {
                    //只处理token过期这一种情况
                    if (response.HttpCode == 401 && response.ErrCode == ErrorCode.ApiTokenExpired)
                    {
                        bool refreshSuccessed = await RefreshJwtAsync(endpoint).ConfigureAwait(false);

                        if (refreshSuccessed)
                        {
                            await SendAsync(request).ConfigureAwait(false);
                        }
                    }
                }

                if (!response.IsSuccessful)
                {
                    throw new ApiException(response.ErrCode, response.Message);
                }
            }
            catch (ApiException)
            {
                throw;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                throw new ApiException(ErrorCode.ApiError, $"ApiClient.SendAsync Failed.", ex);
            }
        }

        public async Task<T> SendAsync<T>(ApiRequest request) where T : class
        {
            await SetDeviceInfoAlwaysAsync(request).ConfigureAwait(false);

            if (!request.IsValid())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError);
            }

            try
            {
                EndpointSettings endpoint = _options.Endpoints.Single(e => e.ProductName == request.GetProductName() && e.Version == request.GetApiVersion());

                HttpClient httpClient = GetHttpClient(endpoint);

                if (request is JwtApiRequest jwtApiRequest)
                {
                    bool jwtAdded = await TrySetJwtAsync(jwtApiRequest).ConfigureAwait(false);

                    if (!jwtAdded)
                    {
                        throw new ApiException(ErrorCode.ApiNoAuthority);
                    }
                }

                if (request is ApiKeyRequest apiKeyRequest)
                {
                    if (!TrySetApiKey(apiKeyRequest))
                    {
                        throw new ApiException(ErrorCode.ApiNoAuthority);
                    }
                }

                ApiResponse<T> response = await request.GetResponseAsync<T>(httpClient).ConfigureAwait(false);

                if (request is JwtApiRequest)
                {
                    //只处理token过期这一种情况
                    if (response.HttpCode == 401 && response.ErrCode == ErrorCode.ApiTokenExpired)
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
                    throw new ApiException(response.ErrCode, response.Message);
                }

                //TODO: 小心检查随后
                return response.Data!;
            }
            catch (ApiException)
            {
                throw;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                throw new ApiException(ErrorCode.ApiError, $"ApiClient.SendAsync Failed.Type : {typeof(T)}", ex);
            }
        }

        private HttpClient GetHttpClient(EndpointSettings endpoint)
        {
            return _httpClientFactory.CreateClient(EndpointSettings.GetHttpClientName(endpoint));
        }

        private static async Task<bool> TrySetJwtAsync(JwtApiRequest request)
        {
            string? accessToken = await ClientGlobal.GetAccessTokenAsync().ConfigureAwait(false);

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

        private static async Task SetDeviceInfoAlwaysAsync(ApiRequest request)
        {
            request.DeviceId = await ClientGlobal.GetDeviceIdAsync().ConfigureAwait(false);
            request.DeviceInfos = ClientGlobal.DeviceInfos;
            request.DeviceVersion = ClientGlobal.DeviceVersion;
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

                string? accessToken = await ClientGlobal.GetAccessTokenAsync().ConfigureAwait(false);

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

                string? refreshToken = await ClientGlobal.GetRefreshTokenAsync().ConfigureAwait(false);

                if (!refreshToken.IsNullOrEmpty())
                {
                    //开始刷新
                    RefreshJwtApiRequest refreshRequest = new RefreshJwtApiRequest(
                        endpointSettings.JwtSettings!.ProductName!,
                        endpointSettings.JwtSettings!.Version!,
                        HttpMethod.Put,
                        endpointSettings.JwtSettings!.ResourceName!,
                        accessToken!,
                        refreshToken!
                        );

                    await SetDeviceInfoAlwaysAsync(refreshRequest).ConfigureAwait(false);

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

                        await ClientGlobal.OnJwtRefreshSucceedAsync(newAccessToken).ConfigureAwait(false);

                        return true;
                    }
                }

                //刷新失败
                _lastRefreshTokenResults[accessTokenHashKey] = false;

                await ClientGlobal.OnJwtRefreshFailedAsync().ConfigureAwait(false);

                return false;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ErrorCode.ApiTokenRefresherError, "ApiClient.AutoRefreshTokenAsync Error.", ex);
            }
            finally
            {
                _tokenRefreshSemaphore.Release();
            }
        }

        private class RefreshJwtApiRequest : ApiRequest
        {
            public string AccessToken { get; set; } = null!;

            public string RefreshToken { get; set; } = null!;

            public RefreshJwtApiRequest(string productName, string apiVersion, HttpMethod httpMethod, string resourceName, string accessToken, string refreshToken) : base(productName, apiVersion, httpMethod, resourceName, null)
            {
                AccessToken = accessToken;
                RefreshToken = refreshToken;
            }
        }


        #endregion
    }
}
