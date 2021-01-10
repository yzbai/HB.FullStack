using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HB.FullStack.Mobile.Api
{
    internal class ApiClient : IApiClient
    {
        private readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClient(IOptions<ApiClientOptions> options, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
        }

        public event AsyncEventHandler<ApiRequest, ApiEventArgs> Requesting
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<object, ApiEventArgs> Responsed
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        /// <exception cref="ApiException"></exception>
        public async Task<IEnumerable<T>> GetAsync<T>(ApiRequest<T> request) where T : ApiResource
            => await SendAsync<T, IEnumerable<T>>(request, ApiRequestType.Get).ConfigureAwait(false) ?? new List<T>();

        /// <exception cref="ApiException"></exception>
        public Task<T?> GetSingleAsync<T>(ApiRequest<T> request) where T : ApiResource
            => SendAsync<T, T>(request, ApiRequestType.GetSingle);

        /// <exception cref="ApiException"></exception>
        public Task AddAsync<T>(AddRequest<T> addRequest) where T : ApiResource
            => SendAsync<T, IEnumerable<long>>(addRequest, ApiRequestType.Add);

        /// <exception cref="ApiException"></exception>
        public Task UpdateAsync<T>(UpdateRequest<T> request) where T : ApiResource
            => SendAsync<T, EmptyResponse>(request, ApiRequestType.Update);

        /// <exception cref="ApiException"></exception>
        public Task DeleteAsync<T>(DeleteRequest<T> request) where T : ApiResource
            => SendAsync<T, EmptyResponse>(request, ApiRequestType.Delete);

        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestType"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        private async Task<TResponse?> SendAsync<T, TResponse>(ApiRequest<T> request, ApiRequestType requestType) where T : ApiResource where TResponse : class
        {
            if (!request.IsValid())
            {
                throw new ApiException(ApiErrorCode.ModelValidationError, request.GetValidateErrorMessage());
            }

            EndpointSettings endpoint = GetEndpoint(request);

            AddDeviceInfo(request);
            AddAuthInfo(request);

            try
            {
                HttpClient httpClient = GetHttpClient(endpoint);

                await OnRequestingAsync(request, new ApiEventArgs(requestType, request)).ConfigureAwait(false);

                TResponse? rt = await httpClient.SendAsync<T, TResponse>(request).ConfigureAwait(false);

                await OnResponsedAsync(rt, new ApiEventArgs(requestType, request)).ConfigureAwait(false);

                return rt;
            }
            catch (ApiException ex)
            {
                if (request.GetApiAuthType() == ApiAuthType.Jwt && ex.HttpCode == HttpStatusCode.Unauthorized && ex.ErrorCode == ApiErrorCode.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpoint).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await SendAsync<T, TResponse>(request, requestType).ConfigureAwait(false);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ApiErrorCode.ClientError, $"ApiClient.SendAsync Failed.Type : {typeof(T)}", ex);
            }
        }

        private Task OnRequestingAsync(ApiRequest apiRequest, ApiEventArgs apiEventArgs)
        {
            return _asyncEventManager.RaiseEventAsync(nameof(Requesting), apiRequest, apiEventArgs);
        }

        private Task OnResponsedAsync(object? responsedObj, ApiEventArgs apiEventArgs)
        {
            return _asyncEventManager.RaiseEventAsync(nameof(Responsed), responsedObj, apiEventArgs);
        }

        private EndpointSettings GetEndpoint<T>(ApiRequest<T> request) where T : ApiResource
        {
            return _options.Endpoints.Single(e => e.Name == request.GetEndpointName() && e.Version == request.GetApiVersion());
        }

        private HttpClient GetHttpClient(EndpointSettings endpoint)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(endpoint.GetHttpClientName());

            httpClient.Timeout = _options.HttpClientTimeout;

            return httpClient;
        }

        private static void AddDeviceInfo(ApiRequest request)
        {
            request.DeviceId = DevicePreferences.DeviceId;
            request.DeviceInfos = DevicePreferences.DeviceInfos;
            request.DeviceVersion = DevicePreferences.DeviceVersion;
            //request.DeviceAddress = await _mobileGlobal.GetDeviceAddressAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// AddAuthInfo
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ApiException"></exception>
        private void AddAuthInfo<T>(ApiRequest<T> request) where T : ApiResource
        {
            switch (request.GetApiAuthType())
            {
                case ApiAuthType.None:
                    break;
                case ApiAuthType.Jwt:

                    if (!TrySetJwt(request))
                    {
                        throw new ApiException(ApiErrorCode.NoAuthority);
                    }
                    break;
                case ApiAuthType.ApiKey:
                    if (!TrySetApiKey(request))
                    {
                        throw new ApiException(ApiErrorCode.NoAuthority);
                    }
                    break;
                default:
                    break;
            }
        }

        private static bool TrySetJwt<T>(ApiRequest<T> request) where T : ApiResource
        {
            if (UserPreferences.AccessToken.IsNullOrEmpty())
            {
                return false;
            }

            request.SetJwt(UserPreferences.AccessToken);

            return true;
        }

        private bool TrySetApiKey<T>(ApiRequest<T> apiRequest) where T : ApiResource
        {
            if (_options.TryGetApiKey(apiRequest.GetApiKeyName(), out string? key))
            {
                apiRequest.SetApiKey(key);
                return true;
            }

            return false;
        }
    }
}
