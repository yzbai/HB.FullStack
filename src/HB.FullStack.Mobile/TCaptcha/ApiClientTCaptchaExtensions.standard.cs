using System;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Mobile.TCaptcha;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Api
{
    public static class ApiClientTCaptchaExtensions
    {
        /// <summary>
        /// GetSingleWithTCaptchaCheckedAsync
        /// </summary>
        /// <param name="apiClient"></param>
        /// <param name="request"></param>
        /// <param name="onSuccessDelegate"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public static async Task GetSingleWithTCaptchaCheckedAsync<T>(this IApiClient apiClient, ApiRequest<T> request, Func<T?, Task>? onSuccessDelegate) where T : ApiResource
        {
            try
            {
                T? resource = await apiClient.GetFirstOrDefaultAsync(request).ConfigureAwait(false);

                if (onSuccessDelegate != null)
                {
                    await onSuccessDelegate(resource).ConfigureAwait(false);
                }
            }
            catch (ApiException ex) when (ex.ErrorCode == ApiErrorCode.ApiPublicResourceTokenNeeded)
            {
                TCaptchaDialog dialog = new TCaptchaDialog(async (result) =>
                {
                    if (result.IsNullOrEmpty())
                    {
                        GlobalSettings.ExceptionHandler.Invoke(new ApiException(ApiErrorCode.ApiCapthaError));
                        return;
                    }

                    request.PublicResourceToken = result;

                    T? resource = await apiClient.GetFirstOrDefaultAsync(request).ConfigureAwait(false);

                    if (onSuccessDelegate != null)
                    {
                        await onSuccessDelegate(resource).ConfigureAwait(false);
                    }
                });

                Device.BeginInvokeOnMainThread(() =>
                {
                    Shell.Current.Navigation.PushModalAsync(dialog, false).Fire();
                });
            }
        }
    }
}