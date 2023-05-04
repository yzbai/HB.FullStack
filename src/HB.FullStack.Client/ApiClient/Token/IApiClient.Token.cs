/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Common.Shared;


namespace HB.FullStack.Client.ApiClient
{
    public static class IApiClientTokenExtensions
    {
        internal static async Task FetchTokenByLoginNameAsync(this IApiClient apiClient, string loginName, string password)
        {
            TokenResGetByLoginNameRequest request = new TokenResGetByLoginNameRequest(
                loginName,
                password,
                apiClient.ApiClientOptions.TokenSiteSetting.SiteName,
                apiClient.TokenPreferences.DeviceInfos);

            TokenRes? res = await apiClient.GetAsync<TokenRes>(request).ConfigureAwait(false);

            ThrowIf.Null(res, "Return a null TokenRes");

            apiClient.TokenPreferences.OnTokenFetched(res);
        }

        internal static async Task FetchTokenBySmsAsync(this IApiClient apiClient, string mobile, string smsCode)
        {
            TokenResGetBySmsRequest request = new TokenResGetBySmsRequest(
                mobile,
                smsCode,
                apiClient.ApiClientOptions.TokenSiteSetting.SiteName,
                apiClient.TokenPreferences.DeviceInfos);

            TokenRes? res = await apiClient.GetAsync<TokenRes>(request).ConfigureAwait(false);

            ThrowIf.Null(res, "Return a null TokenRes");

            apiClient.TokenPreferences.OnTokenFetched(res);
        }

        internal static async Task DeleteTokenAsync(this IApiClient apiClient)
        {
            TokenResDeleteRequest request = new TokenResDeleteRequest();

            await apiClient.SendAsync(request);

            apiClient.TokenPreferences.OnTokenDeleted();
        }

        internal static async Task RefreshTokenAsync(this IApiClient apiClient)
        {
            try
            {
                ITokenPreferences preferenceProvider = apiClient.TokenPreferences;

                if (preferenceProvider.AccessToken.IsNullOrEmpty())
                {
                    throw CommonExceptions.ApiClientInnerError("Can not Refresh your accesstoken if you not logined.", null, null);
                }

                if (preferenceProvider.RefreshToken.IsNullOrEmpty())
                {
                    throw CommonExceptions.ApiClientInnerError("Can not Refresh your accesstoken if you not logined.RefreshToken Empty", null, null);
                }

                TokenResGetByRefreshRequest request = new TokenResGetByRefreshRequest(preferenceProvider.AccessToken!, preferenceProvider.RefreshToken!);

                TokenRes? res = await apiClient.GetAsync<TokenRes>(request).ConfigureAwait(false);

                ThrowIf.Null(res, "Return a null TokenRes");

                apiClient.TokenPreferences.OnTokenFetched(res);
            }
            catch
            {
                apiClient.TokenPreferences.OnTokenRefreshFailed();
                throw;
            }
        }
    }
}