/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HB.FullStack.Client.Components.Users;
using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Client.MauiLib.Controls;
using HB.FullStack.Client.MauiLib.Startup;
using HB.FullStack.Client.MauiLib.Utils;
using HB.FullStack.Common;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Validate;

using Microsoft.Extensions.Options;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Components
{
    public partial class RegisterProfileViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly MauiOptions _options;
        private readonly IUserService _userService;

        private string? _croppedFullPath;
        private bool _avatarChanged;
        private string? _oldNickName;

        [ObservableProperty]
        private ObservableTask<string?>? _avatarFileTask;

        [ObservableProperty]
        private string? _nickName;


        public RegisterProfileViewModel(IOptions<MauiOptions> options, IUserService userProfileService)
        {
            _options = options.Value;
            _userService = userProfileService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(CropperPage.Query_CroppedSuccess, out object? croppedSuccessObj))
            {
                if (croppedSuccessObj is bool croppedSuccess && croppedSuccess)
                {
                    _croppedFullPath = query[CropperPage.Query_CroppedFullPath].ToString()!;

                    AvatarFileTask = new ObservableTask<string?>(_croppedFullPath, null);
                    _avatarChanged = true;
                }
                else
                {
                    //处理剪切失败
                    ShowToast("抱歉，头像剪辑失败，请再试一次。");
                }
            }
        }

        public override async Task OnPageAppearingAsync()
        {
            if (NickName.IsNullOrEmpty())
            {
                _userService.GetNickNameAsync().ContinueWith(nickNameTask =>
                {
                    _oldNickName = nickNameTask.Result;

                    if (_oldNickName.IsNotNullOrEmpty() && !Conventions.IsARandomNickName(_oldNickName))
                    {
                        NickName = _oldNickName;
                    }
                }, TaskScheduler.Default).SafeFireAndForget(ex =>
                {
                    OnExceptionDisplay(ex, "获取您的用户名出错。");
                });
            }

            if (AvatarFileTask == null)
            {
                //使用已有头像
                AvatarFileTask = await _userService.GetAvatarFileObservableTaskAsync(_options.DefaultAvatarFileName);
            }
        }

        public override Task OnPageDisappearingAsync()
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task CropAvatarImageAsync()
        {
            await NavigationHelper.GotoCropPageAsync();
        }

        public bool IsNickNameNotNull => NickName.IsNotNullOrEmpty();

        [RelayCommand(CanExecute = nameof(IsNickNameNotNull))]
        private async Task UpdateProfileAsync()
        {
            IsBusy = true;

            try
            {
                string? updatedNickName = null;

                if (ValidationMethods.IsNickName(NickName) && NickName != _oldNickName)
                {
                    updatedNickName = NickName;
                    _oldNickName = NickName;
                }

                await _userService.UpdateUserProfileAsync(updatedNickName, null, null, _avatarChanged ? _croppedFullPath : null);
                _avatarChanged = false;

                //Navigation
                await Currents.Navigation.GoBackAsync();
            }
            catch (Exception ex)
            {
                OnExceptionDisplay(ex, "抱歉，更新信息出错，请稍后再试。");
            }

            IsBusy = false;
        }
    }
}