﻿using HB.FullStack.Common;
using HB.FullStack.Common.Api;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.XamarinForms.Api
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

        public JwtEndpointSetting GetDefaultJwtEndpointSetting()
        {
            return _options.DefaultJwtEndpoint;
        }

        public Task<IEnumerable<T>> GetAsync<T>(ApiRequest<T> request) where T : ApiResource2
            => GetAsync(request, CancellationToken.None);

        public async Task<IEnumerable<T>> GetAsync<T>(ApiRequest<T> request, CancellationToken cancellationToken) where T : ApiResource2
            => await SendAsync<T, IEnumerable<T>>(request, ApiRequestType.Get, cancellationToken).ConfigureAwait(false) ?? Array.Empty<T>();

        public Task<T?> GetFirstOrDefaultAsync<T>(ApiRequest<T> request) where T : ApiResource2
            => GetFirstOrDefaultAsync(request, CancellationToken.None);

        public async Task<T?> GetFirstOrDefaultAsync<T>(ApiRequest<T> request, CancellationToken cancellationToken) where T : ApiResource2
            => (await GetAsync(request, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        public Task AddAsync<T>(AddRequest<T> addRequest) where T : ApiResource2
            => AddAsync(addRequest, CancellationToken.None);

        public Task AddAsync<T>(AddRequest<T> addRequest, CancellationToken cancellationToken) where T : ApiResource2
        {
            if (typeof(T) == typeof(LongIdResource))
            {
                return SendAsync<T, IEnumerable<long>>(addRequest, ApiRequestType.Add, cancellationToken);
            }
            else if (typeof(T) == typeof(GuidResource))
            {
                return SendAsync<T, EmptyResponse>(addRequest, ApiRequestType.Add, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync<T>(UpdateRequest<T> request) where T : ApiResource2
            => UpdateAsync(request, CancellationToken.None);

        public Task UpdateAsync<T>(UpdateRequest<T> request, CancellationToken cancellationToken) where T : ApiResource2
            => SendAsync<T, EmptyResponse>(request, ApiRequestType.Update, cancellationToken);

        public Task UpdateFields<T>(UpdateFieldsRequest<T> request) where T : ApiResource2
            =>UpdateFields(request, CancellationToken.None);

        public Task UpdateFields<T>(UpdateFieldsRequest<T> request, CancellationToken cancellationToken) where T : ApiResource2
            => SendAsync<T, EmptyResponse>(request, ApiRequestType.Update, cancellationToken);

        public Task DeleteAsync<T>(DeleteRequest<T> request) where T : ApiResource2
            => DeleteAsync(request, CancellationToken.None);   

        public Task DeleteAsync<T>(DeleteRequest<T> request, CancellationToken cancellationToken) where T : ApiResource2
            => SendAsync<T, EmptyResponse>(request, ApiRequestType.Delete, cancellationToken);

        public Task<Stream> GetStreamAsync(ApiRequest request)
            => GetStreamAsync(request, CancellationToken.None);

        public async Task<Stream> GetStreamAsync(ApiRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ModelValidationError(cause: request.GetValidateErrorMessage());
            }

            EndpointSettings? endpoint = GetEndpoint(request);

            AddDeviceInfo(request);
            AddAuthInfo(request);

            try
            {
                //TODO: 调查这里有必要用using吗
                HttpClient httpClient = GetHttpClient(endpoint);

                await OnRequestingAsync(request, new ApiEventArgs(ApiRequestType.Stream, request)).ConfigureAwait(false);

                Stream stream = await httpClient.GetStreamAsync(request, cancellationToken).ConfigureAwait(false);

                await OnResponsedAsync(stream, new ApiEventArgs(ApiRequestType.Stream, request)).ConfigureAwait(false);

                return stream;
            }
            catch (ErrorCode2Exception ex)
            {
                if (request.ApiAuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpoint).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await GetStreamAsync(request, cancellationToken).ConfigureAwait(false);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                throw ApiExceptions.ClientError(cause: "ApiClient.SendAsync Failed.Type : Get Stream", innerException: ex);
            }
        }

        private async Task<TResponse?> SendAsync<T, TResponse>(ApiRequest<T> request, ApiRequestType requestType, CancellationToken cancellationToken) where T : ApiResource2 where TResponse : class
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ModelValidationError(cause: request.GetValidateErrorMessage());
            }

            EndpointSettings? endpoint = GetEndpoint(request);

            AddDeviceInfo(request);
            AddAuthInfo(request);

            try
            {
                HttpClient httpClient = GetHttpClient(endpoint);

                await OnRequestingAsync(request, new ApiEventArgs(requestType, request)).ConfigureAwait(false);

                TResponse? rt = await httpClient.SendAsync<T, TResponse>(request, cancellationToken).ConfigureAwait(false);

                await OnResponsedAsync(rt, new ApiEventArgs(requestType, request)).ConfigureAwait(false);

                return rt;
            }
            catch (ErrorCode2Exception ex)
            {
                if (request.ApiAuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpoint).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await SendAsync<T, TResponse>(request, requestType, cancellationToken).ConfigureAwait(false);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                throw ApiExceptions.ClientError(cause: $"ApiClient.SendAsync Failed.Type : {typeof(T)}", innerException: ex);
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

        private EndpointSettings? GetEndpoint(ApiRequest request)
        {
            return _options.Endpoints.FirstOrDefault(e =>
                e.Name == request.EndpointName
                    &&
                (
                    e.Version == request.ApiVersion
                        ||
                    (request.ApiVersion.IsNullOrEmpty() && e.Version.IsNullOrEmpty())
                ));
        }

        public const string NO_BASEURL_HTTPCLIENT_NAME = nameof(NO_BASEURL_HTTPCLIENT_NAME);

        private HttpClient GetHttpClient(EndpointSettings? endpoint)
        {
            string httpClientName = endpoint == null ? NO_BASEURL_HTTPCLIENT_NAME : endpoint.HttpClientName;

            HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);

            httpClient.Timeout = _options.HttpClientTimeout;

            return httpClient;
        }

        private static void AddDeviceInfo(ApiRequest request)
        {
            request.DeviceId = DevicePreferences.DeviceId;
            //request.DeviceInfos = DevicePreferences.DeviceInfos;
            request.DeviceVersion = DevicePreferences.DeviceVersion;
            //request.DeviceAddress = await _mobileGlobal.GetDeviceAddressAsync().ConfigureAwait(false);
        }

        /// <exception cref="ApiException"></exception>
        private void AddAuthInfo(ApiRequest request)
        {
            switch (request.ApiAuthType)
            {
                case ApiAuthType.None:
                    break;
                case ApiAuthType.Jwt:

                    if (!TrySetJwt(request))
                    {
                        throw ApiExceptions.NoAuthority();
                    }
                    break;
                case ApiAuthType.ApiKey:
                    if (!TrySetApiKey(request))
                    {
                        throw ApiExceptions.NoAuthority();
                    }
                    break;
                default:
                    break;
            }
        }

        private static bool TrySetJwt(ApiRequest request)
        {
            if (UserPreferences.AccessToken.IsNullOrEmpty())
            {
                return false;
            }

            request.SetJwt(UserPreferences.AccessToken);

            return true;
        }

        private bool TrySetApiKey(ApiRequest apiRequest)
        {
            if (_options.TryGetApiKey(apiRequest.ApiKeyName, out string? key))
            {
                apiRequest.SetApiKey(key);
                return true;
            }

            return false;
        }
    }
}
