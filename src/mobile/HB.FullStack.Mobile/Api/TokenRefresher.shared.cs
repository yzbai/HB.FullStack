using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.XamarinForms.Base;

namespace HB.FullStack.XamarinForms.Api
{
    public static class TokenRefresher
    {
        private static readonly MemorySimpleLocker _requestLimiter = new MemorySimpleLocker();

        private static readonly SemaphoreSlim _lastRefreshResultsAccessSemaphore = new SemaphoreSlim(1, 1);

        private static readonly IDictionary<string, bool> _lastRefreshResults = new Dictionary<string, bool>();

        /// <summary>
        /// RefreshAccessTokenAsync
        /// </summary>
        /// <param name="apiClient"></param>
        /// <param name="endpointSettings"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public static async Task<bool> RefreshAccessTokenAsync(IApiClient apiClient, EndpointSettings? endpointSettings)
        {
            if (UserPreferences.AccessToken.IsNullOrEmpty())
            {
                return false;
            }

            JwtEndpointSetting jwtEndpoint = endpointSettings == null ? apiClient.GetDefaultJwtEndpointSetting() : endpointSettings.JwtEndpoint;

            string accessTokenHashKey = SecurityUtil.GetHash(UserPreferences.AccessToken);

            //这个AccessToken不久前刷新过
            if (!_requestLimiter.NoWaitLock(nameof(RefreshAccessTokenAsync), accessTokenHashKey, TimeSpan.FromSeconds(jwtEndpoint.RefreshIntervalSeconds)))
            {
                //可能已经有人在刷新，等他刷新完
                if(!await _lastRefreshResultsAccessSemaphore.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false))
                {
                    //等待失败
                    BaseApplication.ExceptionHandler(ApiExceptions.TokenRefreshError(cause:"AccessToken 有人刷新过，等待获取结果失败。"));
                    return false;
                }

                try
                {
                    if (_lastRefreshResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult))
                    {
                        return lastRefreshResult;
                    }

                    BaseApplication.ExceptionHandler(ApiExceptions.TokenRefreshError(cause:"AccessToken 有人刷新过，但结果获取为空。"));
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
                if (UserPreferences.RefreshToken.IsNotNullOrEmpty())
                {
                    RefreshAccessTokenRequest refreshRequest = new RefreshAccessTokenRequest(
                        jwtEndpoint.EndpointName!,
                        jwtEndpoint.Version!,
                        HttpMethod.Get,
                        jwtEndpoint.ResourceName!,
                        UserPreferences.AccessToken,
                        UserPreferences.RefreshToken);

                    AccessTokenResource? resource = await apiClient.GetFirstOrDefaultAsync(refreshRequest).ConfigureAwait(false);

                    if (resource != null)
                    {
                        _lastRefreshResults.Clear();
                        _lastRefreshResults[accessTokenHashKey] = true;

                        OnRefreshSucceed(resource);

                        return true;
                    }
                }

                //刷新失败
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;

                OnRefreshFailed();

                return false;
            }
            catch
            {
                //刷新失败
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;

                OnRefreshFailed();

                throw;
            }
            finally
            {
                _lastRefreshResultsAccessSemaphore.Release();
            }
        }

        private static void OnRefreshSucceed(AccessTokenResource resource)
        {
            UserPreferences.AccessToken = resource.AccessToken;
        }

        private static void OnRefreshFailed()
        {
            UserPreferences.Logout();
        }

        private class AccessTokenResource : ApiResource
        {
            public string AccessToken { get; set; } = null!;
        }

        private class RefreshAccessTokenRequest : ApiRequest<AccessTokenResource>
        {
            public string AccessToken { get; set; } = null!;

            public string RefreshToken { get; set; } = null!;

            public RefreshAccessTokenRequest(string endpointName, string apiVersion, HttpMethod httpMethod, string resourceName, string accessToken, string refreshToken)
                : base(httpMethod, ApiAuthType.None, endpointName, apiVersion, resourceName, "ByRefresh")
            {
                AccessToken = accessToken;
                RefreshToken = refreshToken;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(AccessToken, RefreshToken);
            }

            public override string ToDebugInfo()
            {
                return "RefreshAccessTokenRequest";
            }
        }
    }
}
