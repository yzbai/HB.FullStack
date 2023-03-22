using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public static class SignInReceiptRefresher
    {
        private static readonly MemorySimpleLocker _requestLimiter = new MemorySimpleLocker();

        private static readonly SemaphoreSlim _lastRefreshResultsAccessSemaphore = new SemaphoreSlim(1, 1);

        private static readonly IDictionary<string, bool> _lastRefreshResults = new Dictionary<string, bool>();

        public static async Task<bool> RefreshSignInReceiptAsync(this IApiClient apiClient)
        {
            IPreferenceProvider tokenProvider = apiClient.SignInReceiptProvider;

            if (tokenProvider.AccessToken.IsNullOrEmpty())
            {
                return false;
            }

            ResEndpoint? resBinding = apiClient.SignInReceiptResEndpoint;

            if (resBinding == null)
            {
                return false;
            }

            string accessTokenHashKey = SecurityUtil.GetHash(tokenProvider.AccessToken);

            //这个AccessToken不久前刷新过
            if (!_requestLimiter.NoWaitLock(nameof(RefreshSignInReceiptAsync), accessTokenHashKey, TimeSpan.FromSeconds(resBinding.SiteSetting!.SignInReceiptRefreshIntervalSeconds)))
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
                if (tokenProvider.RefreshToken.IsNotNullOrEmpty())
                {
                    SignInReceiptResGetByRefreshRequest refreshRequest = new SignInReceiptResGetByRefreshRequest(
                        tokenProvider.UserId!.Value,
                        tokenProvider.AccessToken,
                        tokenProvider.RefreshToken,
                        tokenProvider.ClientId,
                        tokenProvider.ClientVersion,
                        tokenProvider.DeviceInfos);

                    SignInReceiptRes? res = await apiClient.GetAsync<SignInReceiptRes>(refreshRequest).ConfigureAwait(false);

                    if (res != null)
                    {
                        _lastRefreshResults.Clear();
                        _lastRefreshResults[accessTokenHashKey] = true;

                        OnRefreshSucceed(res, tokenProvider);

                        return true;
                    }
                }

                //刷新失败
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;

                OnRefreshFailed(tokenProvider);

                return false;
            }
            catch
            {
                //刷新失败
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;

                OnRefreshFailed(tokenProvider);

                throw;
            }
            finally
            {
                _lastRefreshResultsAccessSemaphore.Release();
            }
        }

        private static void OnRefreshSucceed(SignInReceiptRes res, IPreferenceProvider preferenceProvider)
        {
            preferenceProvider.OnLogined(
                res.UserId,
                res.CreatedTime,
                res.Mobile,
                res.Email,
                res.LoginName,
                res.AccessToken,
                res.RefreshToken);
        }

        private static void OnRefreshFailed(IPreferenceProvider preferenceProvider)
        {
            preferenceProvider.OnTokenRefreshFailed();
        }
    }
}