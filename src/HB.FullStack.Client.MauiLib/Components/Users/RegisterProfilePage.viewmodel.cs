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

using AsyncAwaitBestPractices;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.Components.Users;
using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Client.MauiLib.Controls;
using HB.FullStack.Client.MauiLib.Utils;
using HB.FullStack.Common;
using HB.FullStack.Common.Files;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Validate;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace HB.FullStack.Client.MauiLib.Components
{
    public class RegisterProfileViewModel : BaseViewModel, IQueryAttributable
    {
        [ObservableProperty]
        private ObservableTask<ImageSource>? _avatarImageSourceTask;

        [ObservableProperty]
        private string? _nickName;

        private bool _avatarChanged;
        private string? _newTempAvatarFile;
        private string? _oldNickName;
        private readonly IUserProfileService _userProfileService;

        public RegisterProfileViewModel(ILogger logger, IUserProfileService userProfileService, ITokenPreferences clientPreferences, IFileManager fileManager) : base(logger, clientPreferences, fileManager)
        {
            _userProfileService = userProfileService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(CropperPage.Query_CroppedSucceed, out object? obj) 
                && bool.TryParse(obj?.ToString(), out bool croppedSucceed))
            {
                if (croppedSucceed)
                {
                    AvatarImageSourceTask = new ObservableTask<ImageSource>(ImageSource.FromFile(_newTempAvatarFile), null);
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
                _userProfileService.GetNickNameAsync().ContinueWith(nickNameTask =>
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

            if (AvatarImageSourceTask == null)
            {
                //使用已有头像
                AvatarImageSourceTask = await GetAvatarImageSourceAsync();
            }
        }

        public override Task OnPageDisappearingAsync()
        {
            return Task.CompletedTask;
        }

        private async Task<ObservableTask<ImageSource>> GetAvatarImageSourceAsync()
        {
            string? initAvatarFileName = await _userProfileService.GetAvatarFileAsync(PreferenceProvider.UserId!.Value, RepoGetMode.LocalForced);

            return FileManager.GetImageSource(
                DirectorySettings.Descriptions.PUBLIC_AVATAR.ToDirectory(null),
                initAvatarFileName,
                () => _userDomainService.GetAvatarFileNameAsync(PreferenceProvider.UserId.Value, RepoGetMode.RemoteForced),
                _userDomainService.DefaultAvatarFileName,
                true);
        }

        [RelayCommand]
        private async Task OnCropAvatarImageAsync()
        {
            _newTempAvatarFullPath = FileManager.GetNewTempFullPath(".png");

            await NavigationHelper.GotoCropPageAsync(_newTempAvatarFullPath);
        }

        public bool IsNickNameNotNull => true;//NickName.IsNotNullOrEmpty();

        [RelayCommand(CanExecute = nameof(IsNickNameNotNull))]
        private async Task OnFinishAsync()
        {
            IsBusy = true;

            try
            {
                string? updatedNickName = null;
                string? updatedAvatrFileName = null;

                //Update Avatar
                if (_avatarChanged)
                {
                    await _userDomainService.SetAvatarFileAsync(_newTempAvatarFullPath!);
                    updatedAvatrFileName = Path.GetFileName(_newTempAvatarFullPath);
                    _avatarChanged = false;
                }

                //Update Profile
                if (ValidationMethods.IsNickName(NickName) && NickName != _oldNickName)
                {
                    updatedNickName = NickName;
                    _oldNickName = NickName;
                }

                await _userDomainService.UpdateUserProfileAsync(updatedNickName, null, null, updatedAvatrFileName);

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