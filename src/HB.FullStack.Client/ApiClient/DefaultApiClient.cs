/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.Shared.Resources;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.ApiClient
{
    /// <summary>
    /// 保持单例复用
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>")]
    public partial class DefaultApiClient : IApiClient
    {
        //private readonly WeakEventManager _eventManager = new WeakEventManager();

        private readonly Type _streamType = typeof(Stream);

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        public ITokenPreferences TokenPreferences { get; set; }

        private IDictionary<string, string> _apiKeys = null!;

        private readonly IDictionary<string, ResEndpoint> _resEndpoints = new Dictionary<string, ResEndpoint>();

        public DefaultApiClient(IOptions<ApiClientOptions> options, IHttpClientFactory httpClientFactory, ITokenPreferences preferenceProvider)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            TokenPreferences = preferenceProvider;

            RangeApiKeys();
            RangeResEndpoints();

            //GlobalApiClientAccessor.ApiClient = this;

            void RangeApiKeys()
            {
                _apiKeys = _options.ApiKeys.ToDictionary(item => item.Name, item => item.Key);
            }
        }

        public void RangeResEndpoints()
        {
            AddResEndpointsFromCode();
            AddResEndpointsFromSignInReceiptSite();
            AddResEndpointFromOtherSites();

            void AddResEndpointsFromCode()
            {
                IEnumerable<Type> resTypes = ReflectionUtil.GetAllTypeByCondition(type => type.IsSubclassOf(typeof(ApiResource)));

                foreach (Type resType in resTypes)
                {
                    ResEndpoint endpoint = new ResEndpoint(resType.Name);

                    //直接把SignInReceiptSite作为默认
                    endpoint.SiteSetting = _options.SignInReceiptSiteSetting;

                    ResEndpointAttribute? attr = resType.GetCustomAttribute<ResEndpointAttribute>();

                    if (attr != null)
                    {
                        endpoint.ResName = attr.ResName ?? endpoint.ResName;
                        endpoint.Type = attr.Type ?? endpoint.Type;
                        endpoint.ControllerOrPlainUrl = attr.ControllerOrPlainUrl ?? endpoint.ControllerOrPlainUrl;
                        endpoint.DefaultReadAuth = attr.DefaultReadAuth ?? endpoint.DefaultReadAuth;
                        endpoint.DefaultWriteAuth = attr.DefaultWriteAuth ?? endpoint.DefaultWriteAuth;
                    }

                    _resEndpoints[endpoint.ResName] = endpoint;
                }
            }

            void AddResEndpointsFromSignInReceiptSite()
            {
                _options.SignInReceiptSiteSetting.SiteName ??= "SignInReceiptSite";

                foreach (ResEndpoint endpoint in _options.SignInReceiptSiteSetting.Endpoints)
                {
                    endpoint.SiteSetting = _options.SignInReceiptSiteSetting;
                    _resEndpoints[endpoint.ResName] = endpoint;
                }

                ResEndpoint signInReCeiptResEndpoint = new ResEndpoint(nameof(TokenRes));
                signInReCeiptResEndpoint.SiteSetting = _options.SignInReceiptSiteSetting;
                _resEndpoints[nameof(TokenRes)] = signInReCeiptResEndpoint;
            }

            void AddResEndpointFromOtherSites()
            {
                foreach (SiteSetting siteSetting in _options.OtherSiteSettings)
                {
                    foreach (ResEndpoint endpoint in siteSetting.Endpoints)
                    {
                        endpoint.SiteSetting = siteSetting;

                        //override attribute of res
                        _resEndpoints[endpoint.ResName] = endpoint;
                    }
                }
            }
        }

        #region Events

        public event Func<ApiRequest, ApiEventArgs, Task>? Requesting;

        public event Func<object?, ApiEventArgs, Task>? Responsed;

        private Task OnRequestingAsync(ApiRequest apiRequest, ApiEventArgs apiEventArgs)
        {
            return Requesting?.Invoke(apiRequest, apiEventArgs) ?? Task.CompletedTask;
        }

        private Task OnResponsedAsync(object? responsedObj, ApiEventArgs apiEventArgs)
        {
            return Responsed?.Invoke(responsedObj, apiEventArgs) ?? Task.CompletedTask;
        }

        #endregion

        public Task SendAsync(ApiRequest request, CancellationToken cancellationToken) => GetAsync<EmptyApiResource>(request, cancellationToken);

        public Task SendAsync(ApiRequest request) => SendAsync(request, CancellationToken.None);

        public Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class => GetAsync<TResponse>(request, CancellationToken.None);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Stream will return to user to handle the dispose")]
        public async Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class
        {
            if (!request.IsValid())
            {
                throw CommonExceptions.ApiModelError("Request没有通过Validate", null, new { ValidateErrorMessage = request.GetValidateErrorMessage() });
            }

            if (!_resEndpoints.TryGetValue(request.ResName, out ResEndpoint? resEndpoint))
            {
                throw CommonExceptions.ApiClientInnerError($"No ResEndpoint for {request.ResName}.", null, null);
            }

            HttpRequestMessageBuilder requestBuilder = new HttpRequestMessageBuilder(resEndpoint, request);

            HttpClient httpClient = GetHttpClient(resEndpoint.SiteSetting!);

            try
            {
                ConfigureRequestBuilder(requestBuilder);

                //NOTICE: 这里没有必要用using. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#httpclient-and-lifetime-management-1

                await OnRequestingAsync(request, new ApiEventArgs(request.RequestId, request.ApiMethod)).ConfigureAwait(false);

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

                await OnResponsedAsync(rt, new ApiEventArgs(request.RequestId, request.ApiMethod)).ConfigureAwait(false);

                return rt;
            }
            catch (ErrorCodeException ex)
            {
                if (requestBuilder.Request.Auth == ApiRequestAuth.JWT && ex.ErrorCode == ErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshSignInReceiptAsync(this, TokenPreferences, _options.SignInReceiptRefreshIntervalSeconds).ConfigureAwait(false);

                    if (refreshSuccessed)
                    {
                        return await GetAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (requestBuilder.Request.Auth == ApiRequestAuth.JWT && ex.ErrorCode == ErrorCodes.AuthorizationNoTokenInStore)
                {
                    //TODO: 重新登陆， 客户端应该针对Authroization开头的ErrorCode进行相应处理
                }

                //ErrorCodes.SmsCacheError

                throw;
            }
            catch (Exception ex)
            {
                throw CommonExceptions.ApiClientInnerError("ApiClient非ErrorCodeException", ex, new { Request = request });
            }
        }

        private void ConfigureRequestBuilder(HttpRequestMessageBuilder requestBuilder)
        {
            requestBuilder.SetClientId(TokenPreferences.ClientId);
            requestBuilder.SetClientVersion(TokenPreferences.ClientVersion);

            ApiRequestAuth auth = requestBuilder.Request.Auth!;

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
                        throw CommonExceptions.ApiAuthenticationError("缺少ApiKey", null, new { ApiKeyName = auth.ApiKeyName, RequeestUri = requestBuilder.BuildUriString() });
                    }

                    break;
                }

                case ApiAuthType.Jwt:
                    if (TokenPreferences.AccessToken.IsNullOrEmpty())
                    {
                        throw CommonExceptions.ApiAuthenticationError("缺少AccessToken", null, new { RequeestUri = requestBuilder.BuildUriString() });
                    }

                    requestBuilder.SetJwt(TokenPreferences.AccessToken);
                    break;

                default:
                    break;
            }
        }

        private HttpClient GetHttpClient(SiteSetting endpointSettings)
        {
            string httpClientName = endpointSettings.GetHttpClientName();

            HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);

            httpClient.Timeout = _options.HttpClientTimeout;

            return httpClient;
        }
    }
}