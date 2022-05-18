using System;
using System.Threading.Tasks;

using HB.FullStack.Client;
using HB.FullStack.Client.UI.Maui.TCaptcha;
using HB.FullStack.Common.Api;

using Microsoft.Maui.Controls;

namespace HB.FullStack.Common.ApiClient
{
    public static class ApiClientTCaptchaExtensions
    {
        public static async Task GetSingleWithTCaptchaCheckedAsync<T>(this IApiClient apiClient, ApiRequest request, Func<T?, Task>? onSuccessDelegate) where T : ApiResource2
        {
            try
            {
                T? resource = await apiClient.GetAsync<T>(request).ConfigureAwait(false);

                if (onSuccessDelegate != null)
                {
                    await onSuccessDelegate(resource).ConfigureAwait(false);
                }
            }
            catch (ApiException ex) when (ex.ErrorCode == ApiErrorCodes.CapthcaNotFound)
            {
                TCaptchaDialog dialog = new TCaptchaDialog(async (result) =>
                {
                    if (result.IsNullOrEmpty())
                    {
                        GlobalSettings.ExceptionHandler.Invoke(new ApiException(ApiErrorCodes.ApiCapthaError));
                        return;
                    }

                    request.RequestBuilder!.Headers.Add(ApiHeaderNames.Captcha, result);

                    T? resource = await apiClient.GetAsync<T>(request).ConfigureAwait(false);

                    if (onSuccessDelegate != null)
                    {
                        await onSuccessDelegate(resource).ConfigureAwait(false);
                    }
                });

               await INavigationManager.Current!.PushModalAsync(dialog, false).ConfigureAwait(false);
            }
        }
    }
}