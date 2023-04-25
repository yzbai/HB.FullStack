/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Components.Sms;
using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Common.Files;
using HB.FullStack.Common.Validate;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Dispatching;

namespace HB.FullStack.Client.MauiLib.Components
{
    [QueryProperty(nameof(Mobile), nameof(Mobile))]
    [QueryProperty(nameof(SmsCodeLength), nameof(SmsCodeLength))]
    public partial class SmsVerifyViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly ISmsService _smsService;
        [ObservableProperty]
        private string? _mobile;

        [ObservableProperty]
        private string? _smsCode;

        [ObservableProperty]
        private int _smsCodeLength = 6;

        [ObservableProperty]
        private int _countingDownNumber = 60;

        private bool _isAppearing;

        public SmsVerifyViewModel(ILogger<SmsVerifyViewModel> logger, ITokenPreferences clientPreferences, IFileManager fileManager, IApiClient apiClient, ISmsService smsService) : base(logger, clientPreferences, fileManager)
        {
            _apiClient = apiClient;
            _smsService = smsService;
        }

        public override Task OnPageAppearingAsync()
        {
            _isAppearing = true;

            StartCountingDown();

            return Task.CompletedTask;
        }

        public override Task OnPageDisappearingAsync()
        {
            _isAppearing = false;
            return Task.CompletedTask;
        }

        public bool IsSmsCode => ValidationMethods.IsSmsCode(SmsCode, SmsCodeLength);

        [RelayCommand(CanExecute = nameof(IsSmsCode))]
        private async Task ConfirmSmsCodeAsync()
        {
            IsBusy = true;

            try
            {
                await _apiClient.LoginBySmsAsync(Mobile!, SmsCode!);

                await NavigationHelper.OnSmsCodeVerifiedAsync();
            }
            catch (ErrorCodeException errEx)
            {
                OnExceptionDisplay(errEx, errEx.Message);
            }
            catch (Exception ex)
            {
                OnExceptionDisplay(ex, "UnKown Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public bool IsMobile => ValidationMethods.IsMobilePhone(Mobile);


        //Remark:当RelayCommand的名字相同时，android下会生成warning
        [RelayCommand(CanExecute = nameof(IsMobile))]
        private async Task ReRequestSmsCodeAsync()
        {
            IsBusy = true;

            try
            {
                _ = await _smsService.RequestVCodeAsync(Mobile!);

                StartCountingDown();
            }
            catch (Exception ex)
            {
                OnExceptionDisplay(ex, "Sms requset error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void StartCountingDown()
        {
            CountingDownNumber = 60;

            Currents.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                CountingDownNumber--;

                return _isAppearing && CountingDownNumber != 0;
            });
        }
    }
}