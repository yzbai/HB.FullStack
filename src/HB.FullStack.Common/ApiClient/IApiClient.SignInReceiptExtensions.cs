using System;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.SignInReceipt;

namespace HB.FullStack.Common.ApiClient
{
    public static class IApiClientSignInReceiptExtensions
    {
        public static async Task RegisterByLoginNameAsync(this IApiClient apiClient, string loginName, string password, string audience)
        {
            SignInReceiptResRegisterByLoginNameRequest registerRequest = new SignInReceiptResRegisterByLoginNameRequest(
                loginName,
                password,
                audience,
                apiClient.PreferenceProvider.DeviceInfos);

            await apiClient.SendAsync(registerRequest).ConfigureAwait(false);

            //TODO
            //apiClient.PreferenceProvider.OnRegistered();
        }

        public static async Task LoginByLoginNameAsync(this IApiClient apiClient, string loginName, string password, string audience)
        {
            SignInReceiptResGetByLoginNameRequest request = new SignInReceiptResGetByLoginNameRequest(
                loginName,
                password,
                audience,
                apiClient.PreferenceProvider.DeviceInfos);
            
            await PerformLoginAsync(apiClient, request).ConfigureAwait(false);
        }

        

        public static async Task LoginBySmsAsync(this IApiClient apiClient, string mobile, string smsCode, string audience)
        {
            SignInReceiptResGetBySmsRequest request = new SignInReceiptResGetBySmsRequest(
                mobile,
                smsCode,
                audience,
                apiClient.PreferenceProvider.DeviceInfos);

            await PerformLoginAsync(apiClient, request).ConfigureAwait(false);
        }

        public static async Task UnLoginAsync(this IApiClient apiClient)
        {
            SignInReceiptResDeleteRequest request = new SignInReceiptResDeleteRequest();

            await apiClient.SendAsync(request).ConfigureAwait(false);

            apiClient.PreferenceProvider.OnLogouted();
        }

        public static async Task RefreshSignInReceiptAsync(this IApiClient apiClient)
        {
            IPreferenceProvider preferenceProvider = apiClient.PreferenceProvider;

            if (!preferenceProvider.IsLogined())
            {
                throw CommonExceptions.ApiClientInnerError("Can not Refresh your accesstoken if you not logined.", null, null);
            }

            if (!preferenceProvider.UserId.HasValue)
            {
                throw CommonExceptions.ApiClientInnerError("Can not Refresh your accesstoken if you do not have a userid", null, null);
            }

            SignInReceiptResGetByRefreshRequest request = new SignInReceiptResGetByRefreshRequest(
                preferenceProvider.UserId.Value,
                preferenceProvider.AccessToken!,
                preferenceProvider.RefreshToken!,
                preferenceProvider.DeviceInfos);

            await PerformLoginAsync(apiClient, request).ConfigureAwait(false);
        }

        private static async Task PerformLoginAsync(IApiClient apiClient, ApiRequest request)
        {
            SignInReceiptRes? res = await apiClient.GetAsync<SignInReceiptRes>(request).ConfigureAwait(false);

            ThrowIf.Null(res, "Return a null SignInReceiptRes");

            apiClient.PreferenceProvider.OnLogined(
                userId: res.UserId,
                userCreateTime: res.CreatedTime,
                mobile: res.Mobile,
                email: res.Email,
                loginName: res.LoginName,
                accessToken: res.AccessToken,
                refreshToken: res.RefreshToken);
        }
    }
}
