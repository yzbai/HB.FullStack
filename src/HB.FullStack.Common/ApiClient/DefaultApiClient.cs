using HB.FullStack.Common.Api;
using HB.FullStack.Common.Api.Requests;
using HB.FullStack.Common.Api.Resources;
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

        public JwtEndpointSetting GetLoginJwtEndpointSetting()
        {
            return _options.LoginJwtEndpoint;
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

        //public Task<Stream> GetStreamAsync(ApiRequest request)
        //{
        //    return GetStreamAsync(request, CancellationToken.None);
        //}

        //public async Task<Stream> GetStreamAsync(ApiRequest request, CancellationToken cancellationToken)
        //{
        //    if (!request.IsValid())
        //    {
        //        throw ApiExceptions.ApiRequestInvalidateError(request, request.GetValidateErrorMessage());
        //    }

        //    HttpRequestBuilder requestBuilder = request.CreateHttpRequestBuilder();
        //    EndpointSettings? endpointSettings = GetEndpointSettings(requestBuilder);
        //    HttpClient httpClient = GetHttpClient(endpointSettings);

        //    try
        //    {

        //        ApplyEndpointSettings(requestBuilder, endpointSettings);
        //        ApplyTokenInfo(requestBuilder);

        //        //NOTICE: 这里没有必要用using. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#httpclient-and-lifetime-management-1

        //        await OnRequestingAsync(request, new ApiEventArgs(request.RequestId, request.ApiMethodName)).ConfigureAwait(false);

        //        Stream stream = await httpClient.GetStreamAsync(request, requestBuilder, cancellationToken).ConfigureAwait(false);

        //        await OnResponsedAsync(stream, new ApiEventArgs(request.RequestId, request.ApiMethodName)).ConfigureAwait(false);

        //        return stream;
        //    }
        //    catch (ErrorCode2Exception ex)
        //    {
        //        if (requestBuilder.AuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
        //        {
        //            bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpointSettings, _tokenProvider).ConfigureAwait(false);

        //            if (refreshSuccessed)
        //            {
        //                return await GetStreamAsync(request, cancellationToken).ConfigureAwait(false);
        //            }
        //        }

        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ApiExceptions.ApiClientGetStreamUnkownError(request, innerException: ex);
        //    }
        //}

        public Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class => GetAsync<TResponse>(request, CancellationToken.None);

        Type _streamType = typeof(Stream);

        public async Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ApiRequestInvalidateError(request, request.GetValidateErrorMessage());
            }

            HttpRequestBuilder requestBuilder = request.GetHttpRequestBuilder();
            EndpointSettings? endpointSettings = GetEndpointSettings(requestBuilder);
            HttpClient httpClient = GetHttpClient(endpointSettings);

            try
            {
                ApplyEndpointSettings(requestBuilder, endpointSettings);
                ApplyTokenInfo(requestBuilder);

                //NOTICE: 这里没有必要用using. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#httpclient-and-lifetime-management-1

                await OnRequestingAsync(request, new ApiEventArgs(request.RequestId, request.ApiMethodName)).ConfigureAwait(false);

                TResponse? rt;

                if (_streamType == typeof(TResponse))
                {
#pragma warning disable CA2000 // 会返回，由用户处理
                    Stream stream = await httpClient.GetStreamAsync(request, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA2000 // Dispose objects before losing scope
                    rt = stream as TResponse;
                }
                else
                {
                    rt = await httpClient.GetAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
                }

                await OnResponsedAsync(rt, new ApiEventArgs(request.RequestId, request.ApiMethodName)).ConfigureAwait(false);

                return rt;
            }
            catch (ErrorCode2Exception ex)
            {
                if (requestBuilder.Auth.AuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpointSettings, _tokenProvider).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await GetAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (requestBuilder.Auth.AuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AuthorizationNoTokenInStore)
                {
                    //TODO: 重新登陆， 客户端应该针对Authroization开头的ErrorCode进行相应处理
                }

                //ApiErrorCodes.SmsCacheError

                throw;
            }
            catch (Exception ex)
            {
                throw ApiExceptions.ApiClientUnkownError($"ApiClient.SendAsync Failed.", request, ex);
            }
        }

        public Task SendAsync(ApiRequest request, CancellationToken cancellationToken) => GetAsync<EmptyApiResource>(request, cancellationToken);

        public Task SendAsync(ApiRequest request) => SendAsync(request, CancellationToken.None);

        private static void ApplyEndpointSettings(HttpRequestBuilder requestBuilder, EndpointSettings? endpointSettings)
        {
            requestBuilder.EndpointSettings.HttpMethodOverrideMode = endpointSettings?.HttpMethodOverrideMode ?? HttpMethodOverrideMode.None;

            if (endpointSettings != null && endpointSettings.Challenge.IsNotNullOrEmpty())
            {
                requestBuilder.EndpointSettings.Challenge = endpointSettings.Challenge;
            }
        }

        private void ApplyTokenInfo(HttpRequestBuilder requestBuilder)
        {
            requestBuilder.SetDeviceId(_tokenProvider.DeviceId);
            requestBuilder.SetDeviceVersion(_tokenProvider.DeviceVersion);

            //Auto
            switch (requestBuilder.Auth.AuthType)
            {
                case ApiAuthType.ApiKey:
                    {
                        ThrowIf.NullOrEmpty(requestBuilder.Auth.ApiKeyName, nameof(RestfulHttpRequestBuilder.Auth.ApiKeyName));

                        if (_options.TryGetApiKey(requestBuilder.Auth.ApiKeyName, out string? key))
                        {
                            requestBuilder.SetApiKey(key);
                        }
                        else
                        {
                            throw ApiExceptions.ApiRequestSetApiKeyError(requestBuilder);
                        }

                        break;
                    }

                case ApiAuthType.Jwt:
                    if (_tokenProvider.AccessToken.IsNullOrEmpty())
                    {
                        throw ApiExceptions.ApiRequestSetJwtError(requestBuilder);
                    }

                    requestBuilder.SetJwt(_tokenProvider.AccessToken);
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

        private EndpointSettings? GetEndpointSettings(HttpRequestBuilder requestBuilder)
        {
            EndpointSettings? endpointSettings = null;

            if (requestBuilder is RestfulHttpRequestBuilder restfulApiRequestBuilder)
            {
                //TODO: 用字典提高效率
                endpointSettings = _options.Endpoints.FirstOrDefault(e =>
                    e.EndpointName == restfulApiRequestBuilder.EndpointName
                        &&
                    (
                        e.Version == restfulApiRequestBuilder.ApiVersion
                            ||
                        (restfulApiRequestBuilder.ApiVersion.IsNullOrEmpty() && e.Version.IsNullOrEmpty())
                    ));
            }

            if (endpointSettings == null && _options.DefaultEndpointName.IsNotNullOrEmpty())
            {
                endpointSettings = _options.Endpoints.FirstOrDefault(e => e.EndpointName == _options.DefaultEndpointName);
            }

            return endpointSettings;
        }
    }
}