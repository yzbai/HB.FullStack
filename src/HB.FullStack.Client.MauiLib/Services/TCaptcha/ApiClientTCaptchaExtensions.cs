using System;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.Shared;

using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Services.TCaptcha
{
    public static class ApiClientTCaptchaExtensions
    {
        public static async Task<T?> GetWithCaptchaCheckAsync<T>(this IApiClient apiClient, ApiRequest request, CancellationToken? cancellationToken = null) where T : ApiResource
        {
            try
            {
                return await apiClient.GetAsync<T>(request, cancellationToken ?? CancellationToken.None);

            }
            catch (ErrorCodeException ex) when (ex.ErrorCode == ErrorCodes.CapthcaNotFound)
            {
                TCaptchaPopup popup = new TCaptchaPopup();

                var rt = await popup.ShowAsync();

                string? captcha = rt?.ToString();

                if (captcha.IsNullOrEmpty())
                {
                    throw MauiExceptions.CaptchaErrorReturn(captcha, request);
                }

                request.Headers.Add(ApiHeaderNames.Captcha, captcha);

                return await apiClient.GetAsync<T>(request, cancellationToken ?? CancellationToken.None);
            }

        }
    }
}