using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public static class TokenRefresher
    {
        private static readonly MemorySimpleLocker _requestLimiter = new MemorySimpleLocker();

        private static readonly SemaphoreSlim _lastRefreshResultsAccessSemaphore = new SemaphoreSlim(1, 1);

        private static readonly IDictionary<string, bool> _lastRefreshResults = new Dictionary<string, bool>();

        public static async Task<bool> RefreshAccessTokenAsync(IApiClient apiClient, EndpointSettings? endpointSettings, IPreferenceProvider userTokenProvider)
        {
            if (userTokenProvider.AccessToken.IsNullOrEmpty())
            {
                return false;
            }

            JwtEndpointSetting jwtEndpoint = endpointSettings == null ? apiClient.GetDefaultJwtEndpointSetting() : endpointSettings.JwtEndpoint;

            string accessTokenHashKey = SecurityUtil.GetHash(userTokenProvider.AccessToken);

            //这个AccessToken不久前刷新过
            if (!_requestLimiter.NoWaitLock(nameof(RefreshAccessTokenAsync), accessTokenHashKey, TimeSpan.FromSeconds(jwtEndpoint.RefreshIntervalSeconds)))
            {
                //可能已经有人在刷新，等他刷新完
                if (!await _lastRefreshResultsAccessSemaphore.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false))
                {
                    //等待失败
                    //BaseApplication.ExceptionHandler(ApiExceptions.TokenRefreshError(cause: "AccessToken 有人刷新过，等待获取结果失败。"));
                    return false;
                }

                try
                {
                    if (_lastRefreshResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult))
                    {
                        return lastRefreshResult;
                    }

                    //BaseApplication.ExceptionHandler(ApiExceptions.TokenRefreshError(cause: "AccessToken 有人刷新过，但结果获取为空。"));
                    return false;
                }
                finally
                {
                    _lastRefreshResultsAccessSemaphore.Release();
                }
            }

            //开始刷新，其他想取结果的人等着
            await _lastRefreshResultsAccessSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (userTokenProvider.RefreshToken.IsNotNullOrEmpty())
                {
                    RefreshUserTokenRequest refreshRequest = new RefreshUserTokenRequest(
                        jwtEndpoint.EndpointName!,
                        jwtEndpoint.Version!,
                        jwtEndpoint.ResName!,
                        userTokenProvider.AccessToken,
                        userTokenProvider.RefreshToken);

                    AccessTokenResource? resource = await apiClient.GetAsync<AccessTokenResource>(refreshRequest).ConfigureAwait(false);

                    if (resource != null)
                    {
                        _lastRefreshResults.Clear();
                        _lastRefreshResults[accessTokenHashKey] = true;

                        OnRefreshSucceed(resource, userTokenProvider);

                        return true;
                    }
                }

                //刷新失败
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;

                OnRefreshFailed(userTokenProvider);

                return false;
            }
            catch
            {
                //刷新失败
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;

                OnRefreshFailed(userTokenProvider);

                throw;
            }
            finally
            {
                _lastRefreshResultsAccessSemaphore.Release();
            }
        }

        private static void OnRefreshSucceed(AccessTokenResource resource, IPreferenceProvider userTokenProvider)
        {
            userTokenProvider.OnTokenFetched(
                resource.UserId,
                resource.CreatedTime,
                resource.Mobile,
                resource.Email,
                resource.LoginName,
                resource.AccessToken,
                resource.RefreshToken);
        }

        private static void OnRefreshFailed(IPreferenceProvider userTokenProvider)
        {
            userTokenProvider.OnTokenRefreshFailed();
        }

        private class AccessTokenResource : ApiResource2
        {
            public Guid UserId { get; set; }

            public string? Mobile { get; set; }

            public string? LoginName { get; set; }

            public string? Email { get; set; }

            public DateTimeOffset CreatedTime { get; set; }

            public string AccessToken { get; set; } = null!;

            public string RefreshToken { get; set; } = null!;

            public override int GetHashCode()
            {
                return HashCode.Combine(nameof(AccessTokenResource), AccessToken, RefreshToken, UserId, Mobile, LoginName, Email, CreatedTime);
            }
        }

        private class RefreshUserTokenRequest : ApiRequest
        {
            private readonly string? _endPointName;
            private readonly string? _apiVersion;
            private readonly string? _resName;

            public string AccessToken { get; set; } = null!;

            public string RefreshToken { get; set; } = null!;

            public RefreshUserTokenRequest(string? endPointName, string? apiVersion, string? resName, string accessToken, string refreshToken)
                : base(HttpMethodName.Get, new ApiRequestAuth { AuthType = ApiAuthType.None }, "ByRefresh")
            {
                _endPointName = endPointName;
                _apiVersion = apiVersion;
                _resName = resName;

                AccessToken = accessToken;
                RefreshToken = refreshToken;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(base.GetHashCode(), AccessToken, RefreshToken);
            }

            protected override ApiRequestBuilder CreateBuilder()
            {
                return new RestfulApiRequestBuilder(
                    httpMethod: HttpMethodName,
                    auth: Auth,
                    endPointName: _endPointName,
                    apiVersion: _apiVersion,
                    resName: _resName,
                    condition: Condition);
            }
        }
    }
}