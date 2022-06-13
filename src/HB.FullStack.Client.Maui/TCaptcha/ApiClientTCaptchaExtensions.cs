using System;
using System.Threading;
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
        public static async Task<T?> GetWithTCaptchaCheckAsync<T>(this IApiClient apiClient, ApiRequest request, CancellationToken? cancellationToken = null) where T : ApiResource2
        {
            try
            {
                return await apiClient.GetAsync<T>(request, cancellationToken ?? CancellationToken.None);

            }
            catch (ApiException ex) when (ex.ErrorCode == ApiErrorCodes.CapthcaNotFound)
            {
                TCaptchaPopup popup = new TCaptchaPopup();

                var captcha = await popup.ShowAsync();

                request.RequestBuilder!.Headers.Add(ApiHeaderNames.Captcha, captcha!.ToString()!);

                return await apiClient.GetAsync<T>(request, cancellationToken ?? CancellationToken.None);
            }
        }
    }
}