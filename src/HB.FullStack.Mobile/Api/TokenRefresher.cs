using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Client.Api
{
    public static class TokenRefresher
    {
        private static readonly MemorySimpleLocker _locker = new MemorySimpleLocker();

        private static readonly IDictionary<string, bool> _lastRefreshResults = new Dictionary<string, bool>();

        public static async Task<bool> RefreshAccessTokenAsync(IApiClient apiClient, EndpointSettings endpointSettings)
        {
            string? accessToken = await UserPreferences.GetAccessTokenAsync().ConfigureAwait(false);

            if (accessToken.IsNullOrEmpty())
            {
                return false;
            }

            string accessTokenHashKey = SecurityUtil.GetHash(accessToken!);

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
                string? refreshToken = await UserPreferences.GetRefreshTokenAsync().ConfigureAwait(false);

                if (!refreshToken.IsNullOrEmpty())
                {
                    RefreshAccessTokenRequest refreshRequest = new RefreshAccessTokenRequest(
                        endpointSettings.JwtEndpoint!.EndpointName!,
                        endpointSettings.JwtEndpoint!.Version!,
                        HttpMethod.Get,
                        endpointSettings.JwtEndpoint!.ResourceName!,
                        accessToken!,
                        refreshToken!);

                    AccessTokenResource? resource = await apiClient.GetSingleAsync(refreshRequest).ConfigureAwait(false);

                    if (resource != null)
                    {
                        _lastRefreshResults.Clear();
                        _lastRefreshResults[accessTokenHashKey] = true;

                        await OnRefreshSucceedAsync(resource).ConfigureAwait(false);

                        return true;
                    }
                }

                //刷新失败
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;

                await OnRefreshFailedAsync().ConfigureAwait(false);

                return false;
            }
            catch
            {
                //刷新失败
                _lastRefreshResults.Clear();
                _lastRefreshResults[accessTokenHashKey] = false;

                await OnRefreshFailedAsync().ConfigureAwait(false);

                throw;
            }
        }

        private static async Task OnRefreshSucceedAsync(AccessTokenResource resource)
        {
            await UserPreferences.SetAccessTokenAsync(resource.AccessToken).ConfigureAwait(false);
        }

        private static async Task OnRefreshFailedAsync()
        {
            await UserPreferences.SetRefreshTokenAsync(null).ConfigureAwait(false);
            await UserPreferences.SetAccessTokenAsync(null).ConfigureAwait(false);
        }

        private class AccessTokenResource : Resource
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
        }
    }
}
