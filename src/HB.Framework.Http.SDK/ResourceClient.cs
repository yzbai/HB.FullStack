using HB.Framework.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HB.Framework.Http.SDK
{
    public class ResourceClient : IResourceClient
    {
        //move to settings
        private const string RefreshTokenFrequencyCheckResource = "_Fqc_Refresh";

        private static readonly SemaphoreSlim tokenRefreshSemaphore = new SemaphoreSlim(1, 1);

        private readonly ILogger logger;

        private readonly ResourceClientOptions options;

        private readonly ILocalStorage localStorage;

        private readonly IHttpClientFactory httpClientFactory;

        private readonly IDeviceInfoProvider deviceInfoProvider;

        private readonly InMemoryFrequencyChecker frequencyChecker = new InMemoryFrequencyChecker();

        private readonly IDictionary<string, bool> lastRefreshTokenResults = new Dictionary<string, bool>();

        public ResourceClient(IOptions<ResourceClientOptions> options, ILogger<ResourceClient> logger, ILocalStorage localStorage, IDeviceInfoProvider deviceInfoProvider, IHttpClientFactory httpClientFactory)
        {
            this.options = options.ThrowIfNull(nameof(options)).Value;
            this.logger = logger;
            this.localStorage = localStorage.ThrowIfNull(nameof(localStorage));
            this.deviceInfoProvider = deviceInfoProvider.ThrowIfNull(nameof(deviceInfoProvider));
            this.httpClientFactory = httpClientFactory;
        }

        //多次尝试，自动refresh token，
        public async Task<Resource<T>> GetAsync<T>(ResourceRequest request) where T : ResourceResponse
        {
            ThrowIf.Null(request, nameof(request));

            if (!request.IsValid())
            {
                return new RequestValidateErrorResponse<T>(request);
            }

            if (!await AddAuthenticateIfNeededAsync(request).ConfigureAwait(false))
            {
                return new NotLoginResponse<T>();
            }

            AddDeviceInfoAlways(request);

            Resource<T> response = await GetResponseCore<T>(request).ConfigureAwait(false);

            return await AutoRefreshTokenAsync(request, response).ConfigureAwait(false);
        }

        private async Task<Resource<T>> GetResponseCore<T>(ResourceRequest request) where T : ResourceResponse
        {
            using (HttpRequestMessage httpRequest = ConstructureHttpRequest(request))
            {
                HttpClient httpClient = GetHttpClient(request.GetProductType());

                using (HttpResponseMessage httpResponse = await GetResponseActual(httpRequest, httpClient).ConfigureAwait(false))
                {
                    return await ConstructureHttpResponseAsync<T>(httpResponse).ConfigureAwait(false);
                }
            }
        }

        private async Task<HttpResponseMessage> GetResponseActual(HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            TraceLog(httpRequestMessage, httpResponseMessage);

            return httpResponseMessage;
        }

        private void TraceLog(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse)
        {
            logger.LogTrace($"Request {httpRequest.RequestUri}, Response {httpResponse.StatusCode}");
        }

        private async Task<Resource<T>> AutoRefreshTokenAsync<T>(ResourceRequest request, Resource<T> response) where T : ResourceResponse
        {
            if (response?.HttpCode != 401 || response?.ErrCode != ErrorCode.API_TOKEN_EXPIRED || !request.GetNeedAuthenticate())
            {
                return response;
            }

            //只处理token过期这一种情况

            await tokenRefreshSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                string accessToken = await localStorage.GetAccessTokenAsync().ConfigureAwait(false);

                if (accessToken.IsNullOrEmpty())
                {
                    return response;
                }

                string accessTokenHashKey = SecurityUtil.GetHash(accessToken);

                //不久前刷新过
                if (!frequencyChecker.Check(RefreshTokenFrequencyCheckResource, accessTokenHashKey, TimeSpan.FromSeconds(options.TokenRefreshIntervalSeconds)))
                {
                    if (lastRefreshTokenResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult) && lastRefreshResult)
                    {
                        //刷新成功，再次调用
                        return await GetAsync<T>(request).ConfigureAwait(false);
                    }

                    return response;
                }

                //开始刷新
                string refreshToken = await localStorage.GetRefreshTokenAsync().ConfigureAwait(false);

                if (!refreshToken.IsNullOrEmpty())
                {
                    ResourceRequest refreshRequest = new ResourceRequest(
                        options.TokenRefreshSettings.ProductType,
                        options.TokenRefreshSettings.Version,
                        HttpMethod.Put,
                        false,
                        options.TokenRefreshSettings.ResourceName);

                    refreshRequest.AddParameter(options.TokenRefreshSettings.AccessTokenParameterName, accessToken);
                    refreshRequest.AddParameter(options.TokenRefreshSettings.RefreshTokenParameterName, refreshToken);

                    HttpClient httpClient = GetHttpClient(options.TokenRefreshSettings.ProductType);

                    using (HttpRequestMessage httpRefreshRequest = ConstructureHttpRequest(refreshRequest))
                    {
                        using (HttpResponseMessage refreshResponse = await GetResponseActual(httpRefreshRequest, httpClient).ConfigureAwait(false))
                        {
                            if (refreshResponse.StatusCode == HttpStatusCode.OK)
                            {
                                lastRefreshTokenResults[accessTokenHashKey] = true;

                                string jsonString = await refreshResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                                string newAccessToken = JsonUtil.FromJson(jsonString, options.TokenRefreshSettings.RefreshTokenParameterName);

                                await localStorage.SetAccessTokenAsync(newAccessToken).ConfigureAwait(false);

                                return await GetAsync<T>(request).ConfigureAwait(false);
                            }
                        }
                    }
                }

                //刷新失败
                //frequencyChecker.Reset(frequencyCheckResourceName, accessTokenHash);
                lastRefreshTokenResults[accessTokenHashKey] = false;

                await localStorage.SetAccessTokenAsync(null).ConfigureAwait(false);
                await localStorage.SetRefreshTokenAsync(null).ConfigureAwait(false);


                return response;
            }
            finally
            {
                tokenRefreshSemaphore.Release();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private static HttpRequestMessage ConstructureHttpRequest(ResourceRequest request)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(request.GetHttpMethod(), ConstructureRequestUrl(request));

            if (request.GetHttpMethod() != HttpMethod.Get)
            {
                httpRequest.Content = new FormUrlEncodedContent(request.GetParameters());
            }

            request.GetHeaders().ForEach(kv => httpRequest.Headers.Add(kv.Key, kv.Value));

            return httpRequest;
        }

        private static string ConstructureRequestUrl(ResourceRequest request)
        {
            StringBuilder requestUrlBuilder = new StringBuilder();

            if (!request.GetApiVersion().IsNullOrEmpty())
            {
                requestUrlBuilder.Append(request.GetApiVersion());
            }

            if (!request.GetResourceName().IsNullOrEmpty())
            {
                requestUrlBuilder.Append("/");
                requestUrlBuilder.Append(request.GetResourceName());
            }

            if (!request.GetCondition().IsNullOrEmpty())
            {
                requestUrlBuilder.Append("/");
                requestUrlBuilder.Append(request.GetCondition());
            }

            if (request.GetHttpMethod() == HttpMethod.Get)
            {
                string query = request.GetParameters().ToHttpValueCollection().ToString();

                if (!query.IsNullOrEmpty())
                {
                    requestUrlBuilder.Append("?");
                    requestUrlBuilder.Append(query);
                }
            }

            return requestUrlBuilder.ToString();
        }

        private async Task<Resource<T>> ConstructureHttpResponseAsync<T>(HttpResponseMessage httpResponse) where T : ResourceResponse
        {
            ThrowIf.Null(httpResponse, nameof(httpResponse));

            using (Stream responseStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                if (httpResponse.IsSuccessStatusCode)
                {
                    T resource = JsonUtil.FromStream<T>(responseStream);

                    return new Resource<T>(resource, (int)httpResponse.StatusCode);
                }
                else
                {
                    ErrorResponse errorResponse = JsonUtil.FromStream<ErrorResponse>(responseStream);

                    return new Resource<T>((int)httpResponse.StatusCode, errorResponse.Message, errorResponse.Code);
                }
            }
        }

        private HttpClient GetHttpClient(string productType)
        {
            return httpClientFactory.CreateClient(productType);
        }

        private async Task<bool> AddAuthenticateIfNeededAsync(ResourceRequest request)
        {
            if (request.GetNeedAuthenticate())
            {
                string accessToken = await localStorage.GetAccessTokenAsync().ConfigureAwait(false);

                if (accessToken.IsNullOrEmpty())
                {
                    return false;
                }

                request.AddHeader("Authorization", "Bearer " + accessToken);
            }

            return true;
        }

        private void AddDeviceInfoAlways(ResourceRequest request)
        {
            request.AddParameter("DeviceId", deviceInfoProvider.DeviceId);
            request.AddParameter("DeviceType", deviceInfoProvider.DeviceType);
            request.AddParameter("DeviceVersion", deviceInfoProvider.DeviceVersion);
            request.AddParameter("DeviceAddress", deviceInfoProvider.DeviceAddress);
        }
    }
}
