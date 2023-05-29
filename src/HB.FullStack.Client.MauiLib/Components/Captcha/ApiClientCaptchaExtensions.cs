/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.Shared;

using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Components
{
    public static class ApiClientCaptchaExtensions
    {
        public static async Task<T?> GetWithCaptchaCheckAsync<T>(this IApiClient apiClient, ApiRequest request, CancellationToken? cancellationToken = null) 
            where T : class, ISharedResource
        {
            try
            {
                return await apiClient.GetAsync<T>(request, cancellationToken ?? CancellationToken.None);
            }
            catch (ErrorCodeException ex) when (ex.ErrorCode == ErrorCodes.CapthcaNotFound)
            {
                CaptchaPopup popup = new CaptchaPopup();

                var rt = await popup.ShowAsync();

                string? captcha = rt?.ToString();

                if (captcha.IsNullOrEmpty())
                {
                    throw MauiExceptions.CaptchaErrorReturn(captcha, request);
                }

                request.Headers.Add(SharedNames.ApiHeaders.Captcha, captcha);

                return await apiClient.GetAsync<T>(request, cancellationToken ?? CancellationToken.None);
            }
        }
    }
}