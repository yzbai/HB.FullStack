using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;

namespace HB.FullStack.XamarinForms.Api
{
    public static class TokenRefresher
    {
        private static readonly MemorySimpleLocker _locker = new MemorySimpleLocker();

        private static readonly IDictionary<string, bool> _lastRefreshResults = new Dictionary<string, bool>();

        /// <summary>
        /// RefreshAccessTokenAsync
        /// </summary>
        /// <param name="apiClient"></param>
        /// <param name="endpointSettings"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public static async Task<bool> RefreshAccessTokenAsync(IApiClient apiClient, EndpointSettings endpointSettings)
        {
            if (UserPreferences.AccessToken.IsNullOrEmpty())
            {
                return false;
            }

            string accessTokenHashKey = SecurityUtil.GetHash(UserPreferences.AccessToken);

            //这个AccessToken不久前刷新过
            if (!_locker.NoWaitLock(
                nameof(RefreshAccessTokenAsync),
                accessTokenHashKey,
                TimeSpan.FromSeconds(endpointSettings.JwtEndpoint.RefreshIntervalSeconds)))
            {
                if (_lastRefreshResults.TryGetValue(accessTokenHashKey, out bool lastRefreshResult))
                {
                    return lastRefreshResult;
                }

                return false;
            }

            //开始刷新
            try
            {
                if (UserPreferences.RefreshToken.IsNotNullOrEmpty())
                {
                    RefreshAccessTokenRequest refreshRequest = new RefreshAccessTokenRequest(
                        endpointSettings.JwtEndpoint!.EndpointName!,
                        endpointSettings.JwtEndpoint!.Version!,
                        HttpMethod.Get,
                        endpointSettings.JwtEndpoint!.ResourceName!,
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
                : base(ApiAuthType.None, httpMethod, "ByRefresh", endpointName, apiVersion, resourceName)
            {
                AccessToken = accessToken;
                RefreshToken = refreshToken;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(AccessToken, RefreshToken);
            }
        }
    }
}
