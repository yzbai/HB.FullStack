/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HB.FullStack.Client.Components.Sms;
using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Client.MauiLib.Controls;
using HB.FullStack.Client.MauiLib.Startup;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Validate;

using Microsoft.Extensions.Options;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Components
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly MauiOptions _options;
        private readonly ISmsService _smsService;

        [ObservableProperty]
        private string? _mobile;

        public bool IsMobile => ValidationMethods.IsMobilePhone(Mobile);

        [RelayCommand(CanExecute = nameof(IsMobile))]
        private async Task RequestSmsCodeAsync()
        {
            IsBusy = true;

            try
            {
                SmsValidationCodeRes? res = await _smsService.RequestVCodeAsync(Mobile!);

                ThrowIf.Null(res, "");

                await NavigationHelper.GotoSmsVerifyPageAsync(Mobile!, res.Length);
            }
            catch (MauiException mex) when (mex.ErrorCode == ErrorCodes.CaptchaErrorReturn)
            {
                OnExceptionDisplay(mex, "验证码出错，请稍后再试！");
            }
            catch (Exception ex)
            {
                OnExceptionDisplay(ex, "请求短信出错");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ShowPrivacyAgreementAsync()
        {
            WebviewPopup webviewDialog = new WebviewPopup
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                Url = _options.UrlOfPrivacyAgreement!
            };

            await webviewDialog.ShowAsync();
        }

        [RelayCommand]
        private async Task ShowServieAgreementAsync()
        {
            WebviewPopup webviewDialog = new WebviewPopup
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                Url = _options.UrlOfServiceAgreement!
            };

            await webviewDialog.ShowAsync();
        }

        public LoginViewModel(IOptions<MauiOptions> options, ISmsService smsService)
        {
            _options = options.Value;
            _smsService = smsService;
        }

        public override Task OnPageAppearingAsync()
        {
            return Task.CompletedTask;
        }

        public override Task OnPageDisappearingAsync()
        {
            return Task.CompletedTask;
        }
    }
}