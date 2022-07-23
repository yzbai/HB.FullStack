using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

using Microsoft.Extensions.Options;

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

        private IDictionary<string, string> _apiKeys = null!;

        private readonly Type _streamType = typeof(Stream);

        private readonly IDictionary<string, ResBinding> _resBindings = new Dictionary<string, ResBinding>();

        public IPreferenceProvider UserTokenProvider { get; }

        public ResBinding? UserTokenResBinding { get; private set; }

        public DefaultApiClient(IOptions<ApiClientOptions> options, IHttpClientFactory httpClientFactory, IPreferenceProvider tokenProvider)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            UserTokenProvider = tokenProvider;

            RangeApiKeys();
            RangeEndpoints();

            GlobalApiClientAccessor.ApiClient = this;

            if (_resBindings.TryGetValue(nameof(UserTokenRes), out ResBinding? userTokenResBinding))
            {
                UserTokenResBinding = userTokenResBinding;
            }

            void RangeApiKeys()
            {
                _apiKeys = _options.ApiKeys.ToDictionary(item => item.Name, item => item.Key);
            }

            void RangeEndpoints()
            {
                foreach (var endpoint in _options.EndpointSettings)
                {
                    foreach (var binding in endpoint.ResBindings)
                    {
                        if (!_resBindings.TryAdd(binding.ResName, binding))
                        {
                            throw ApiExceptions.ApiClientInnerError("Multiple ResBinding Defined!", null, new { ResBinding = binding });
                        }

                        binding.EndpointSetting = endpoint;
                    }
                }
            }
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
        public Task SendAsync(ApiRequest request, CancellationToken cancellationToken) => GetAsync<EmptyApiResource>(request, cancellationToken);

        public Task SendAsync(ApiRequest request) => SendAsync(request, CancellationToken.None);

        public Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class => GetAsync<TResponse>(request, CancellationToken.None);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Stream will return to user to handle the dispose")]
        public async Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ApiModelError("Request没有通过Validate", null, new { ValidateErrorMessage = request.GetValidateErrorMessage() });
            }

            if (!_resBindings.TryGetValue(request.ResName, out ResBinding? resBinding))
            {
                throw ApiExceptions.ApiClientInnerError($"No ResBinding for {request.ResName}.", null, null);
            }

            HttpRequestBuilder requestBuilder = new HttpRequestBuilder(resBinding, request);

            HttpClient httpClient = GetHttpClient(resBinding.EndpointSetting!);

            try
            {
                ApplyTokenInfo(requestBuilder);

                //NOTICE: 这里没有必要用using. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#httpclient-and-lifetime-management-1

                await OnRequestingAsync(request, new ApiEventArgs(request.RequestId, request.ApiMethodName)).ConfigureAwait(false);

                TResponse? rt;

                if (_streamType == typeof(TResponse))
                {
                    Stream stream = await httpClient.GetStreamAsync(request, requestBuilder, cancellationToken).ConfigureAwait(false);
                    rt = stream as TResponse;
                }
                else
                {
                    rt = await httpClient.GetAsync<TResponse>(request, requestBuilder, cancellationToken).ConfigureAwait(false);
                }

                await OnResponsedAsync(rt, new ApiEventArgs(request.RequestId, request.ApiMethodName)).ConfigureAwait(false);

                return rt;
            }
            catch (ErrorCode2Exception ex)
            {
                if (requestBuilder.Request.Auth == ApiRequestAuth2.JWT && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await this.RefreshUserTokenAsync().ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await GetAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (requestBuilder.Request.Auth == ApiRequestAuth2.JWT && ex.ErrorCode == ApiErrorCodes.AuthorizationNoTokenInStore)
                {
                    //TODO: 重新登陆， 客户端应该针对Authroization开头的ErrorCode进行相应处理
                }

                //ApiErrorCodes.SmsCacheError

                throw;
            }
            catch (Exception ex)
            {
                throw ApiExceptions.ApiClientInnerError("ApiClient非ErrorCodeException", ex, new { Request = request });
            }
        }

        private void ApplyTokenInfo(HttpRequestBuilder requestBuilder)
        {
            requestBuilder.SetDeviceId(UserTokenProvider.DeviceId);
            requestBuilder.SetDeviceVersion(UserTokenProvider.DeviceVersion);

            ApiRequestAuth2 auth = requestBuilder.Request.Auth!;

            //Auto
            switch (auth.AuthType)
            {
                case ApiAuthType.ApiKey:
                    {
                        ThrowIf.NullOrEmpty(auth.ApiKeyName, "ApiKeyName");

                        if (_apiKeys.TryGetValue(auth.ApiKeyName, out string? key))
                        {
                            requestBuilder.SetApiKey(key);
                        }
                        else
                        {
                            throw ApiExceptions.ApiAuthenticationError("缺少ApiKey", null, new { ApiKeyName = auth.ApiKeyName, RequeestUri = requestBuilder.AssembleUrl() });
                        }

                        break;
                    }

                case ApiAuthType.Jwt:
                    if (UserTokenProvider.AccessToken.IsNullOrEmpty())
                    {
                        throw ApiExceptions.ApiAuthenticationError("缺少AccessToken", null, new { RequeestUri = requestBuilder.AssembleUrl() });
                    }

                    requestBuilder.SetJwt(UserTokenProvider.AccessToken);
                    break;

                default:
                    break;
            }
        }

        private HttpClient GetHttpClient(EndpointSetting endpointSettings)
        {
            string httpClientName = endpointSettings.GetHttpClientName();

            HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);

            httpClient.Timeout = _options.HttpClientTimeout;

            return httpClient;
        }
    }
}