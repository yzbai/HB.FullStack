using System;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.XamarinForms.Base;
using HB.FullStack.XamarinForms.TCaptcha;

using Xamarin.Forms;

namespace HB.FullStack.Client.TCaptcha
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
            catch (ApiException ex) when (ex.ErrorCode == ApiErrorCodes.PublicResourceTokenNeeded)
            {
                TCaptchaDialog dialog = new TCaptchaDialog(async (result) =>
                {
                    if (result.IsNullOrEmpty())
                    {
                        GlobalSettings.ExceptionHandler.Invoke(new ApiException(ApiErrorCodes.ApiCapthaError));
                        return;
                    }

                    request.PublicResourceToken = result;

                    T? resource = await apiClient.GetAsync<T>(request).ConfigureAwait(false);

                    if (onSuccessDelegate != null)
                    {
                        await onSuccessDelegate(resource).ConfigureAwait(false);
                    }
                });

                await NavigationService.Current.GotoAsync(dialog, false).ConfigureAwait(false);
            }
        }
    }
}