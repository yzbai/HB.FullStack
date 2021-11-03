using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Api.Requests;

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

        public Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class
            => GetAsync<TResponse>(request, CancellationToken.None);

        public Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class
            => GetResponseAsync<TResponse>(request, ApiRequestType.Get, cancellationToken);

        public Task<TRes?> GetByIdAsync<TRes>(Guid id) where TRes : ApiResource2
            => GetByIdAsync<TRes>(id, CancellationToken.None);

        public Task<TRes?> GetByIdAsync<TRes>(Guid id, CancellationToken cancellationToken) where TRes : ApiResource2
            => GetResponseAsync<TRes>(new GetByIdRequest<TRes>(id), ApiRequestType.GetById, cancellationToken);

        public Task<IEnumerable<TRes>> GetAllAsync<TRes>() where TRes : ApiResource2
            => GetAllAsync<TRes>(CancellationToken.None);

        public async Task<IEnumerable<TRes>> GetAllAsync<TRes>(CancellationToken cancellationToken) where TRes : ApiResource2
            => (await GetResponseAsync<IEnumerable<TRes>>(new GetAllRequest<TRes>(), ApiRequestType.GetAll, cancellationToken).ConfigureAwait(false))!;

        public Task AddAsync<T>(AddRequest<T> addRequest) where T : ApiResource2
            => AddAsync(addRequest, CancellationToken.None);

        public Task AddAsync<T>(AddRequest<T> addRequest, CancellationToken cancellationToken) where T : ApiResource2
        {
            if (typeof(T) == typeof(LongIdResource))
            {
                return GetResponseAsync<IEnumerable<long>>(addRequest, ApiRequestType.Add, cancellationToken);
            }
            else if (typeof(T) == typeof(GuidResource))
            {
                return GetResponseAsync<EmptyResponse>(addRequest, ApiRequestType.Add, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync<T>(UpdateRequest<T> request) where T : ApiResource2
            => UpdateAsync(request, CancellationToken.None);

        public Task UpdateAsync<T>(UpdateRequest<T> request, CancellationToken cancellationToken) where T : ApiResource2
            => GetResponseAsync<EmptyResponse>(request, ApiRequestType.Update, cancellationToken);

        public Task UpdateFieldsAsync<T>(UpdateFieldsRequest<T> request) where T : ApiResource2
            => UpdateFieldsAsync(request, CancellationToken.None);

        public Task UpdateFieldsAsync<T>(UpdateFieldsRequest<T> request, CancellationToken cancellationToken) where T : ApiResource2
            => GetResponseAsync<EmptyResponse>(request, ApiRequestType.UpdateFields, cancellationToken);

        public Task DeleteAsync<T>(DeleteRequest<T> request) where T : ApiResource2
            => DeleteAsync(request, CancellationToken.None);

        public Task DeleteAsync<T>(DeleteRequest<T> request, CancellationToken cancellationToken) where T : ApiResource2
            => GetResponseAsync<EmptyResponse>(request, ApiRequestType.Delete, cancellationToken);

        public Task<Stream> GetStreamAsync(ApiRequest request)
            => GetStreamAsync(request, CancellationToken.None);

        public async Task<Stream> GetStreamAsync(ApiRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ApiRequestInvalidateError(request, request.GetValidateErrorMessage());
            }

            EndpointSettings? endpoint = GetEndpoint(request);

            AddDeviceInfo(request);
            AddAuthInfo(request);

            try
            {
                // 这里没有必要用using
                //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#httpclient-and-lifetime-management-1

                HttpClient httpClient = GetHttpClient(endpoint);

                await OnRequestingAsync(request, new ApiEventArgs(ApiRequestType.GetStream, request)).ConfigureAwait(false);

                Stream stream = await httpClient.GetStreamAsync(request, cancellationToken).ConfigureAwait(false);

                await OnResponsedAsync(stream, new ApiEventArgs(ApiRequestType.GetStream, request)).ConfigureAwait(false);

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
                throw ApiExceptions.ApiClientGetStreamUnkownError(request, innerException: ex);
            }
        }

        private async Task<TResponse?> GetResponseAsync<TResponse>(ApiRequest request, ApiRequestType requestType, CancellationToken cancellationToken) where TResponse : class
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ApiRequestInvalidateError(request, request.GetValidateErrorMessage());
            }

            EndpointSettings? endpoint = GetEndpoint(request);

            AddDeviceInfo(request);
            AddAuthInfo(request);

            try
            {
                HttpClient httpClient = GetHttpClient(endpoint);

                await OnRequestingAsync(request, new ApiEventArgs(requestType, request)).ConfigureAwait(false);

                TResponse? rt = await httpClient.GetResponseAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);

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
                        return await GetResponseAsync<TResponse>(request, requestType, cancellationToken).ConfigureAwait(false);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                throw ApiExceptions.ApiClientUnkownError($"ApiClient.SendAsync Failed.", request, ex);
            }
        }

        public Task UploadAsync(UploadRequest request)
            => UploadAsync(request, CancellationToken.None);

        public Task UploadAsync(UploadRequest request, CancellationToken cancellationToken)
            => GetResponseAsync<EmptyResponse>(request, ApiRequestType.Upload, cancellationToken);

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
                        throw ApiExceptions.ApiRequestSetJwtError(request);
                    }
                    break;
                case ApiAuthType.ApiKey:
                    if (!TrySetApiKey(request))
                    {
                        throw ApiExceptions.ApiRequestSetApiKeyError(request);
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
