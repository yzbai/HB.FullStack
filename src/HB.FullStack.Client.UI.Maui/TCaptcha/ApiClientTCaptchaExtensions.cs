using System;
using System.Threading.Tasks;

using HB.FullStack.Client;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.XamarinForms.Base;
using HB.FullStack.XamarinForms.Navigation;
using HB.FullStack.XamarinForms.TCaptcha;

using Microsoft.Maui.Controls;

using Xamarin.Forms;

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

                INavigationManager navigation

                await Shell.Current.Navigation.PushModalAsync(dialog, false).ConfigureAwait(false);
            }
        }
    }
}