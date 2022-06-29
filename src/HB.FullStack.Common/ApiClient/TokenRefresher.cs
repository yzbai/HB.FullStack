using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

            JwtEndpointSetting jwtEndpoint = endpointSettings?.JwtEndpoint ?? apiClient.GetDefaultJwtEndpointSetting();

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
                    BearerTokenResGetByRefreshRequest refreshRequest = new BearerTokenResGetByRefreshRequest(
                        jwtEndpoint,
                        userTokenProvider.UserId!.Value,
                        userTokenProvider.AccessToken,
                        userTokenProvider.RefreshToken,
                        userTokenProvider.DeviceId,
                        userTokenProvider.DeviceVersion,
                        userTokenProvider.DeviceInfos);

                    BearerTokenRes? res = await apiClient.GetAsync<BearerTokenRes>(refreshRequest).ConfigureAwait(false);

                    if (res != null)
                    {
                        _lastRefreshResults.Clear();
                        _lastRefreshResults[accessTokenHashKey] = true;

                        OnRefreshSucceed(res, userTokenProvider);

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

        private static void OnRefreshSucceed(BearerTokenRes res, IPreferenceProvider userTokenProvider)
        {
            userTokenProvider.OnTokenFetched(
                res.UserId,
                res.CreatedTime,
                res.Mobile,
                res.Email,
                res.LoginName,
                res.AccessToken,
                res.RefreshToken);
        }

        private static void OnRefreshFailed(IPreferenceProvider userTokenProvider)
        {
            userTokenProvider.OnTokenRefreshFailed();
        }
    }
}