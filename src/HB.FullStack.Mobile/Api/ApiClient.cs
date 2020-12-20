using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Resources;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Api
{
    internal class ApiClient : IApiClient
    {
        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClient(IOptions<ApiClientOptions> options, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T?> GetSingleAsync<T>(ApiRequest<T> request) where T : Resource
        {
            if (!request.IsValid())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError, HttpStatusCode.BadRequest);
            }

            bool isJwtRequest = request is JwtApiRequest<T>;
            EndpointSettings endpoint = GetEndpoint(request);

            AddDeviceInfo(request);
            AddAuthInfo(request);

            try
            {
                HttpClient httpClient = GetHttpClient(endpoint);

                return await httpClient.GetSingleAsync(request).ConfigureAwait(false);
            }
            catch (ApiException ex)
            {
                if (isJwtRequest && ex.HttpCode == HttpStatusCode.Unauthorized && ex.ErrorCode == ErrorCode.ApiTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpoint).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await GetSingleAsync<T>(request).ConfigureAwait(false);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ErrorCode.ApiUnkown, HttpStatusCode.BadRequest, $"ApiClient.SendAsync Failed.Type : {typeof(T)}", ex);
            }
        }

        public async Task<IEnumerable<T>> GetAsync<T>(ApiRequest<T> request) where T : Resource
        {
            if (!request.IsValid())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError, HttpStatusCode.BadRequest);
            }

            bool isJwtRequest = request is JwtApiRequest<T>;
            EndpointSettings endpoint = GetEndpoint(request);

            AddDeviceInfo(request);
            AddAuthInfo(request);

            try
            {

                HttpClient httpClient = GetHttpClient(endpoint);

                IEnumerable<T>? results = await httpClient.GetAsync(request).ConfigureAwait(false);

                return results ?? new List<T>();
            }
            catch (ApiException ex)
            {
                if (isJwtRequest && ex.HttpCode == HttpStatusCode.Unauthorized && ex.ErrorCode == ErrorCode.ApiTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpoint).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await GetAsync(request).ConfigureAwait(false);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ErrorCode.ApiUnkown, HttpStatusCode.BadRequest, $"ApiClient.SendAsync Failed.Type : {typeof(T)}", ex);
            }
        }

        public Task AddAsync<T>(ApiRequest<T> request) where T : Resource => NonQueryAsync(request);

        public Task UpdateAsync<T>(ApiRequest<T> request) where T : Resource => NonQueryAsync(request);

        public Task DeleteAsync<T>(ApiRequest<T> request) where T : Resource => NonQueryAsync(request);

        private async Task NonQueryAsync<T>(ApiRequest<T> request) where T : Resource
        {
            if (!request.IsValid())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError, HttpStatusCode.BadRequest);
            }

            bool isJwtRequest = request is JwtApiRequest<T>;
            EndpointSettings endpoint = GetEndpoint(request);

            try
            {
                AddDeviceInfo(request);
                AddAuthInfo(request);

                HttpClient httpClient = GetHttpClient(endpoint);

                await httpClient.NonQueryAsync(request).ConfigureAwait(false);
            }
            catch (ApiException ex)
            {
                if (isJwtRequest && ex.HttpCode == HttpStatusCode.Unauthorized && ex.ErrorCode == ErrorCode.ApiTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpoint).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        await NonQueryAsync(request).ConfigureAwait(false);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ErrorCode.ApiUnkown, HttpStatusCode.BadRequest, $"ApiClient.SendAsync Failed.Type : {typeof(T)}", ex);
            }
        }

        private EndpointSettings GetEndpoint<T>(ApiRequest<T> request) where T : Resource
        {
            return _options.Endpoints.Single(e => e.Name == request.GetProductName() && e.Version == request.GetApiVersion());
        }

        private HttpClient GetHttpClient(EndpointSettings endpoint)
        {
            return _httpClientFactory.CreateClient(GetHttpClientName(endpoint));
        }

        public static string GetHttpClientName(EndpointSettings endpoint)
        {
            return endpoint.Name + "_" + endpoint.Version;
        }

        private static void AddDeviceInfo(ApiRequest request)
        {
            request.DeviceId = ClientGlobal.GetDeviceId();
            request.DeviceInfos = ClientGlobal.DeviceInfos;
            request.DeviceVersion = ClientGlobal.DeviceVersion;
            //request.DeviceAddress = await _mobileGlobal.GetDeviceAddressAsync().ConfigureAwait(false);
        }

        private void AddAuthInfo<T>(ApiRequest<T> request) where T : Resource
        {
            if (request is JwtApiRequest<T> jwtApiRequest)
            {
                bool jwtAdded = TrySetJwt(jwtApiRequest);

                if (!jwtAdded)
                {
                    throw new ApiException(ErrorCode.ApiNoAuthority, System.Net.HttpStatusCode.Unauthorized);
                }
            }
            else if (request is ApiKeyRequest<T> apiKeyRequest)
            {
                if (!TrySetApiKey(apiKeyRequest))
                {
                    throw new ApiException(ErrorCode.ApiNoAuthority, System.Net.HttpStatusCode.Unauthorized);
                }
            }
        }

        private static bool TrySetJwt<T>(JwtApiRequest<T> request) where T : Resource
        {
            string? accessToken = ClientGlobal.GetAccessToken();

            if (accessToken.IsNullOrEmpty())
            {
                return false;
            }

            request.SetJwt(accessToken!);

            return true;
        }

        private bool TrySetApiKey<T>(ApiKeyRequest<T> apiKeyRequest) where T : Resource
        {
            if (_options.TryGetApiKey(apiKeyRequest.GetApiKeyName(), out string key))
            {
                apiKeyRequest.SetApiKey(key);
                return true;
            }

            return false;
        }
    }
}
