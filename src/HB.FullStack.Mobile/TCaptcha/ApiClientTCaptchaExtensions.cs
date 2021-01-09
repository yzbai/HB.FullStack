using System;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.Resources;
using HB.FullStack.Mobile.TCaptcha;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Api
{
    public static class ApiClientTCaptchaExtensions
    {
        public static async Task GetSingleWithTCaptchaCheckedAsync<T>(this IApiClient apiClient, ApiRequest<T> request, Func<T?, Task>? onSuccessDelegate) where T : Resource
        {
            try
            {
                T? resource = await apiClient.GetSingleAsync(request).ConfigureAwait(false);

                if (onSuccessDelegate != null)
                {
                    await onSuccessDelegate(resource).ConfigureAwait(false);
                }
            }
            catch (ApiException ex) when (ex.ErrorCode == ErrorCode.ApiPublicResourceTokenNeeded)
            {
                TCaptchaDialog dialog = new TCaptchaDialog(async (result) =>
                {
                    if (result.IsNullOrEmpty())
                    {
                        throw new ApiException(ErrorCode.ApiCapthaError, System.Net.HttpStatusCode.BadRequest);
                    }

                    request.PublicResourceToken = result;

                    T? resource = await apiClient.GetSingleAsync(request).ConfigureAwait(false);

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