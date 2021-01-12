﻿using HB.FullStack.Common;
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

        public async Task<IEnumerable<T>> GetAsync<T>(ApiRequest<T> request) where T : Resource
            => await SendAsync<T, IEnumerable<T>>(request, ApiRequestType.Get).ConfigureAwait(false) ?? new List<T>();

        public Task<T?> GetSingleAsync<T>(ApiRequest<T> request) where T : Resource
            => SendAsync<T, T>(request, ApiRequestType.GetSingle);

        public Task AddAsync<T>(AddRequest<T> addRequest) where T : Resource
            => SendAsync<T, IEnumerable<long>>(addRequest, ApiRequestType.Add);

        public Task UpdateAsync<T>(UpdateRequest<T> request) where T : Resource
            => SendAsync<T, EmptyResponse>(request, ApiRequestType.Update);

        public Task DeleteAsync<T>(DeleteRequest<T> request) where T : Resource
            => SendAsync<T, EmptyResponse>(request, ApiRequestType.Delete);

        private async Task<TResponse?> SendAsync<T, TResponse>(ApiRequest<T> request, ApiRequestType requestType) where T : Resource where TResponse : class
        {
            if (!request.IsValid())
            {
                throw new ApiException(ErrorCode.ApiModelValidationError, HttpStatusCode.BadRequest, request.GetValidateErrorMessage());
            }

            EndpointSettings endpoint = GetEndpoint(request);

            AddDeviceInfo(request);
            await AddAuthInfoAsync(request).ConfigureAwait(false);

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
                if (request.GetApiAuthType() == ApiAuthType.Jwt && ex.HttpCode == HttpStatusCode.Unauthorized && ex.ErrorCode == ErrorCode.ApiTokenExpired)
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
                throw new ApiException(ErrorCode.ApiUnkown, HttpStatusCode.BadRequest, $"ApiClient.SendAsync Failed.Type : {typeof(T)}", ex);
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

        private EndpointSettings GetEndpoint<T>(ApiRequest<T> request) where T : Resource
        {
            return _options.Endpoints.Single(e => e.Name == request.GetEndpointName() && e.Version == request.GetApiVersion());
        }

        private HttpClient GetHttpClient(EndpointSettings endpoint)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(endpoint.GetHttpClientName());

            httpClient.Timeout = TimeSpan.FromSeconds(10); //TODO: move to 

            return httpClient;
        }

        private static void AddDeviceInfo(ApiRequest request)
        {
            request.DeviceId = DevicePreferences.GetDeviceId();
            request.DeviceInfos = DevicePreferences.DeviceInfos;
            request.DeviceVersion = DevicePreferences.DeviceVersion;
            //request.DeviceAddress = await _mobileGlobal.GetDeviceAddressAsync().ConfigureAwait(false);
        }

        private async Task AddAuthInfoAsync<T>(ApiRequest<T> request) where T : Resource
        {
            switch (request.GetApiAuthType())
            {
                case ApiAuthType.None:
                    break;
                case ApiAuthType.Jwt:

                    if (!await TrySetJwtAsync(request).ConfigureAwait(false))
                    {
                        throw new ApiException(ErrorCode.ApiNoAuthority, System.Net.HttpStatusCode.Unauthorized);
                    }
                    break;
                case ApiAuthType.ApiKey:
                    if (!TrySetApiKey(request))
                    {
                        throw new ApiException(ErrorCode.ApiNoAuthority, System.Net.HttpStatusCode.Unauthorized);
                    }
                    break;
                default:
                    break;
            }
        }

        private static async Task<bool> TrySetJwtAsync<T>(ApiRequest<T> request) where T : Resource
        {
            string? accessToken = await UserPreferences.GetAccessTokenAsync().ConfigureAwait(false);

            if (accessToken.IsNullOrEmpty())
            {
                return false;
            }

            request.SetJwt(accessToken!);

            return true;
        }

        private bool TrySetApiKey<T>(ApiRequest<T> apiRequest) where T : Resource
        {
            if (_options.TryGetApiKey(apiRequest.GetApiKeyName(), out string key))
            {
                apiRequest.SetApiKey(key);
                return true;
            }

            return false;
        }
    }
}