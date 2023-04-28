/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Common.Shared.Resources;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Client.ApiClient
{
    public static class TokenRefresher
    {
        private static readonly MemorySimpleLocker _requestLimiter = new MemorySimpleLocker();

        private static readonly SemaphoreSlim _lastRefreshResultsAccessSemaphore = new SemaphoreSlim(1, 1);

        private static readonly IDictionary<string, bool> _lastRefreshResults = new Dictionary<string, bool>();

        public static async Task<bool> RefreshTokenAsync(IApiClient apiClient, ITokenPreferences tokenPreferences, int tokenRefreshIntervalSeconds)
        {
            if (tokenPreferences.AccessToken.IsNullOrEmpty())
            {
                return false;
            }

            string accessTokenHashKey = SecurityUtil.GetHash(tokenPreferences.AccessToken);

            //这个AccessToken不久前刷新过
            if (!_requestLimiter.NoWaitLock(nameof(RefreshTokenAsync), accessTokenHashKey, TimeSpan.FromSeconds(tokenRefreshIntervalSeconds)))
            {
                //可能已经有人在刷新，等他刷新完
                if (!await _lastRefreshResultsAccessSemaphore.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false))
                {
                    //等待失败
                    //BaseApplication.ExceptionHandler(CommonExceptions.TokenRefreshError(cause: "AccessToken 有人刷新过，等待获取结果失败。"));
                    return false;
                }

                try
                {
                    if (_lastRefreshResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult))
                    {
                        return lastRefreshResult;
                    }

                    //BaseApplication.ExceptionHandler(CommonExceptions.TokenRefreshError(cause: "AccessToken 有人刷新过，但结果获取为空。"));
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
                await apiClient.RefreshTokenAsync();

                OnRefreshSucceed();

                return true;

            }
            catch
            {
                OnRefreshFailed();

                throw;
            }
            finally
            {
                _lastRefreshResultsAccessSemaphore.Release();
            }

            void OnRefreshSucceed()
            {
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = true;
            }

            void OnRefreshFailed()
            {
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;
            }
        }
    }
}