using HB.FullStack.Common.Api;

using Microsoft.Extensions.Options;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Common.ApiClient
{
    /// <summary>
    /// 保持单例复用
    /// </summary>
    public class DefaultApiClient : IApiClient
    {
        private readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IPreferenceProvider _tokenProvider;

        public DefaultApiClient(IOptions<ApiClientOptions> options, IHttpClientFactory httpClientFactory, IPreferenceProvider tokenProvider)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;

            GlobalApiClientAccessor.ApiClient = this;
        }

        public JwtEndpointSetting GetDefaultJwtEndpointSetting()
        {
            return _options.DefaultJwtEndpoint;
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

        private Task OnRequestingAsync(ApiRequest apiRequest, ApiEventArgs apiEventArgs)
        {
            return _asyncEventManager.RaiseEventAsync(nameof(Requesting), apiRequest, apiEventArgs);
        }

        private Task OnResponsedAsync(object? responsedObj, ApiEventArgs apiEventArgs)
        {
            return _asyncEventManager.RaiseEventAsync(nameof(Responsed), responsedObj, apiEventArgs);
        }

        //public Task AddAsync<T>(AddRequest<T> addRequest, CancellationToken cancellationToken) where T : ApiResource2
        //{
        //    if (typeof(T) == typeof(LongIdResource))
        //    {
        //        return GetAsync<IEnumerable<long>>(addRequest, ApiRequestType.Add, cancellationToken);
        //    }
        //    else if (typeof(T) == typeof(GuidResource))
        //    {
        //        return GetAsync<EmptyResponse>(addRequest, ApiRequestType.Add, cancellationToken);
        //    }

        //    return Task.CompletedTask;
        //}

        public Task<Stream> GetStreamAsync(ApiRequest request)
        {
            return GetStreamAsync(request, CancellationToken.None);
        }

        public async Task<Stream> GetStreamAsync(ApiRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ApiRequestInvalidateError(request, request.GetValidateErrorMessage());
            }

            AddTokenInfo(request);

            try
            {
                //NOTICE: 这里没有必要用using. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#httpclient-and-lifetime-management-1

                EndpointSettings? endpointSettings = GetEndpointSettings(request);

                HttpClient httpClient = GetHttpClient(endpointSettings);

                HttpMethodOverrideMode httpMethodOverrideMode = endpointSettings?.HttpMethodOverrideMode ?? HttpMethodOverrideMode.None;

                await OnRequestingAsync(request, new ApiEventArgs(request.RequestId, request.RequestBuilder!.HttpMethod)).ConfigureAwait(false);

                Stream stream = await httpClient.GetStreamAsync(request, httpMethodOverrideMode, cancellationToken).ConfigureAwait(false);

                await OnResponsedAsync(stream, new ApiEventArgs(request.RequestId, request.RequestBuilder!.HttpMethod)).ConfigureAwait(false);

                return stream;
            }
            catch (ErrorCode2Exception ex)
            {
                if (request.RequestBuilder!.AuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, GetEndpointSettings(request), _tokenProvider).ConfigureAwait(false);

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

        public Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class => GetAsync<TResponse>(request, CancellationToken.None);

        public async Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ApiRequestInvalidateError(request, request.GetValidateErrorMessage());
            }

            AddTokenInfo(request);

            try
            {
                EndpointSettings? endpointSettings = GetEndpointSettings(request);

                HttpClient httpClient = GetHttpClient(endpointSettings);

                HttpMethodOverrideMode httpMethodOverrideMode = endpointSettings?.HttpMethodOverrideMode ?? HttpMethodOverrideMode.None;

                await OnRequestingAsync(request, new ApiEventArgs(request.RequestId, request.RequestBuilder!.HttpMethod)).ConfigureAwait(false);

                TResponse? rt = await httpClient.GetAsync<TResponse>(request, httpMethodOverrideMode, cancellationToken).ConfigureAwait(false);

                await OnResponsedAsync(rt, new ApiEventArgs(request.RequestId, request.RequestBuilder!.HttpMethod)).ConfigureAwait(false);

                return rt;
            }
            catch (ErrorCode2Exception ex)
            {
                if (request.RequestBuilder!.AuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, GetEndpointSettings(request), _tokenProvider).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await GetAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                throw ApiExceptions.ApiClientUnkownError($"ApiClient.SendAsync Failed.", request, ex);
            }
        }

        public Task SendAsync(ApiRequest request, CancellationToken cancellationToken) => GetAsync<EmptyResponse>(request, cancellationToken);

        public Task SendAsync(ApiRequest request) => SendAsync(request, CancellationToken.None);

        private void AddTokenInfo(ApiRequest request)
        {
            request.RequestBuilder!.SetDeviceId(_tokenProvider.DeviceId);
            request.RequestBuilder.SetDeviceVersion(_tokenProvider.DeviceVersion);

            //Auto
            switch (request.RequestBuilder!.AuthType)
            {
                case ApiAuthType.ApiKey:
                    {
                        ThrowIf.NullOrEmpty(request.RequestBuilder!.ApiKeyName, nameof(RestfulHttpRequestBuilder.ApiKeyName));

                        if (_options.TryGetApiKey(request.RequestBuilder!.ApiKeyName, out string? key))
                        {
                            request.RequestBuilder!.SetApiKey(key);
                        }
                        else
                        {
                            throw ApiExceptions.ApiRequestSetApiKeyError(request);
                        }

                        break;
                    }

                case ApiAuthType.Jwt:
                    if (_tokenProvider.AccessToken.IsNullOrEmpty())
                    {
                        throw ApiExceptions.ApiRequestSetJwtError(request);
                    }

                    request.RequestBuilder.SetJwt(_tokenProvider.AccessToken);
                    break;

                default:
                    break;
            }
        }

        private HttpClient GetHttpClient(EndpointSettings? endpointSettings)
        {
            string httpClientName = endpointSettings == null ? ApiClientOptions.NO_BASEURL_HTTPCLIENT_NAME : endpointSettings.HttpClientName;

            HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);

            httpClient.Timeout = _options.HttpClientTimeout;

            return httpClient;
        }
        private EndpointSettings? GetEndpointSettings(ApiRequest request)
        {
            if (request.RequestBuilder is RestfulHttpRequestBuilder defaultBuildInfo)
            {
                //TODO: 用字典提高效率
                return _options.Endpoints.FirstOrDefault(e =>
                    e.Name == defaultBuildInfo.EndpointName
                        &&
                    (
                        e.Version == defaultBuildInfo.ApiVersion
                            ||
                        (defaultBuildInfo.ApiVersion.IsNullOrEmpty() && e.Version.IsNullOrEmpty())
                    ));
            }

            return null;
        }
    }
}