﻿using HB.FullStack.Common;
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

namespace HB.FullStack.Common.ApiClient
{
    public class DefaultApiClient : IApiClient
    {
        private readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();

        private readonly ApiClientOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IApiTokenProvider _tokenProvider;

        public DefaultApiClient(IOptions<ApiClientOptions> options, IHttpClientFactory httpClientFactory, IApiTokenProvider tokenProvider)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
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
        //        return GetResponseAsync<IEnumerable<long>>(addRequest, ApiRequestType.Add, cancellationToken);
        //    }
        //    else if (typeof(T) == typeof(GuidResource))
        //    {
        //        return GetResponseAsync<EmptyResponse>(addRequest, ApiRequestType.Add, cancellationToken);
        //    }

        //    return Task.CompletedTask;
        //}

        public async Task<Stream> GetStreamAsync(ApiRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ApiRequestInvalidateError(request, request.GetValidateErrorMessage());
            }

            EndpointSettings? endpoint = GetEndpoint(request);

            AddTokenInfo(request);

            try
            {
                // 这里没有必要用using
                //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#httpclient-and-lifetime-management-1

                HttpClient httpClient = GetHttpClient(endpoint);

                await OnRequestingAsync(request, new ApiEventArgs(request.RequestId, request.HttpMethod)).ConfigureAwait(false);

                Stream stream = await httpClient.GetStreamAsync(request, cancellationToken).ConfigureAwait(false);

                await OnResponsedAsync(stream, new ApiEventArgs(request.RequestId, request.HttpMethod)).ConfigureAwait(false);

                return stream;
            }
            catch (ErrorCode2Exception ex)
            {
                if (request.ApiAuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpoint, _tokenProvider).ConfigureAwait(false);

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

        public async Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class
        {
            if (!request.IsValid())
            {
                throw ApiExceptions.ApiRequestInvalidateError(request, request.GetValidateErrorMessage());
            }

            EndpointSettings? endpoint = GetEndpoint(request);

            AddTokenInfo(request);

            try
            {
                HttpClient httpClient = GetHttpClient(endpoint);

                await OnRequestingAsync(request, new ApiEventArgs(request.RequestId, request.HttpMethod)).ConfigureAwait(false);

                TResponse? rt = await httpClient.GetResponseAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);

                await OnResponsedAsync(rt, new ApiEventArgs(request.RequestId, request.HttpMethod)).ConfigureAwait(false);

                return rt;
            }
            catch (ErrorCode2Exception ex)
            {
                if (request.ApiAuthType == ApiAuthType.Jwt && ex.ErrorCode == ApiErrorCodes.AccessTokenExpired)
                {
                    bool refreshSuccessed = await TokenRefresher.RefreshAccessTokenAsync(this, endpoint, _tokenProvider).ConfigureAwait(false);

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

        private void AddTokenInfo(ApiRequest request)
        {
            request.DeviceId = _tokenProvider.DeviceId;
            request.DeviceVersion = _tokenProvider.DeviceVersion;

            //Auto
            switch (request.ApiAuthType)
            {
                case ApiAuthType.ApiKey:
                    {
                        ThrowIf.NullOrEmpty(request.ApiKeyName, nameof(ApiRequest.ApiKeyName));

                        if (_options.TryGetApiKey(request.ApiKeyName, out string? key))
                        {
                            request.SetApiKey(key);
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

                    request.SetJwt(_tokenProvider.AccessToken);
                    break;

                default:
                    break;
            }
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

        private HttpClient GetHttpClient(EndpointSettings? endpoint)
        {
            string httpClientName = endpoint == null ? ApiClientOptions.NO_BASEURL_HTTPCLIENT_NAME : endpoint.HttpClientName;

            HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);

            httpClient.Timeout = _options.HttpClientTimeout;

            return httpClient;
        }
    }
}