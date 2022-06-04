using System;
using System.Threading.Tasks;

using CommunityToolkit.Maui.Views;

using HB.FullStack.Client;
using HB.FullStack.Client.Maui.Base;
using HB.FullStack.Client.Maui.TCaptcha;
using HB.FullStack.Client.Navigation;
using HB.FullStack.Common.Api;

using Microsoft.Maui.Controls;

namespace HB.FullStack.Common.ApiClient
{
    public static class ApiClientTCaptchaExtensions
    {
        public static async Task<T?> GetSingleWithTCaptchaCheckedAsync<T>(this IApiClient apiClient, ApiRequest request) where T : ApiResource2
        {
            try
            {
                return await apiClient.GetAsync<T>(request).ConfigureAwait(false);

            }
            catch (ApiException ex) when (ex.ErrorCode == ApiErrorCodes.CapthcaNotFound)
            {
                TCaptchaPopup popup = new TCaptchaPopup();

                var captcha = await popup.ShowAsync().ConfigureAwait(false);

                request.RequestBuilder!.Headers.Add(ApiHeaderNames.Captcha, captcha!.ToString()!);

                return await apiClient.GetAsync<T>(request).ConfigureAwait(false);
            }
        }
    }
}