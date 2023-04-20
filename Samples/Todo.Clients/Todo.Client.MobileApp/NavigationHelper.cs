/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;

using HB.FullStack.Client.MauiLib.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace Todo.Client.MobileApp
{
    public static class NavigationHelper
    {
        /// <summary>
        /// 相对路由, 顶层路由在AppShell中定义
        /// </summary>
        public static void RegisterRouting()
        {
            //From Base Controls
            Routing.RegisterRoute(nameof(CropperPage), typeof(CropperPage));

            //TODO: 考虑SourceGeneration
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(SmsVerifyPage), typeof(SmsVerifyPage));
            Routing.RegisterRoute(nameof(RegisterProfilePage), typeof(RegisterProfilePage));
            Routing.RegisterRoute(nameof(IntroducePage), typeof(IntroducePage));
        }

        private static bool IsPageLoginNeeded(string pageName)
        {
            return pageName switch
            {
                //nameof(TestPage) => false,
                nameof(IntroducePage) => false,
                nameof(LoginPage) => false,
                nameof(SmsVerifyPage) => false,

                nameof(RegisterProfilePage) => true,
                nameof(HomePage) => true,

                _ => TodoAppOptions.NeedLoginDefault
            };
        }

        /// <summary>
        /// 如果需要介绍就进入介绍页，否则正常进入
        /// </summary>
        public static async Task ToptoHomePageAsync()
        {
            await Currents.Shell.GoToAsync($"//{nameof(HomePage)}");
        }

        public static async Task GotoIntroducePageAsync()
        {
            await Currents.Shell.GoToAsync($"{nameof(IntroducePage)}");
        }

        public static async Task OnIntroduceFinishedAsync()
        {
            await Currents.Shell.GoBackAsync();
        }

        private static bool _isLoginPagePushed;

        public static async Task GotoLoginPageAsync()
        {
            await Currents.Shell.GoToAsync($"{nameof(LoginPage)}");
            _isLoginPagePushed = true;
        }

        public static async Task GotoSmsVerifyPageAsync(string mobile)
        {
            await Currents.Shell.GoToAsync($"{nameof(SmsVerifyPage)}?Mobile={mobile}");
        }

        public static async Task OnSmsCodeVerifiedAsync()
        {
            //如果需要就登记信息，否则返回
            bool needRegister = await NeedRegisterProfileAsync();

            string backUrl = _isLoginPagePushed ? "../.." : "..";

            string url = needRegister ? $"{backUrl}/{nameof(RegisterProfilePage)}" : $"{backUrl}";

            await Currents.Shell.GoToAsync(url);

            _isLoginPagePushed = false;
        }

        public static async Task OnRegisterProfileFinishedAsync()
        {
            await Currents.Shell.GoBackAsync();
        }

        public static async Task GotoCropPageAsync(string toSaveFullPath)
        {
            string action = await Currents.Page.DisplayActionSheet("", "取消", null, new string[] { "拍照", "相册" });

            FileResult? photo = action switch
            {
                "拍照" => await MediaPicker.CapturePhotoAsync(),
                "相册" => await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "选择照片" }),
                _ => null,
            };

            if (photo == null)
            {
                return;
            }

            await Currents.Shell.GoToAsync(nameof(CropperPage), new Dictionary<string, object>
            {
                { nameof(CropperViewModel.ImageFullPath), photo.FullPath },
                { nameof(CropperViewModel.CroppedImageFullPath), toSaveFullPath }
            });
        }

        private static IUserDomainService? _userDomainService;
        private static TodoAppOptions? _todoAppOptions;
        private static IPreferenceProvider? _preferenceProvider;

        private static IUserDomainService UserDomainService => _userDomainService ??= Currents.Services.GetRequiredService<IUserDomainService>();

        private static TodoAppOptions TodoAppOptions => _todoAppOptions ??= Currents.Services.GetRequiredService<IOptions<TodoAppOptions>>().Value;

        private static IPreferenceProvider PreferenceProvider => _preferenceProvider ??= Currents.Services.GetRequiredService<IPreferenceProvider>();

        public static async Task<bool> NeedRegisterProfileAsync()
        {
            return !await UserDomainService.NeedRegisterProfileAsync();
        }

        public static bool NeedLogin(string pageName)
        {
            return !PreferenceProvider.IsLogined() && IsPageLoginNeeded(pageName);
        }

        //不用每次访问VersionTracking和UserPreferences，提高效率
        private static bool _firstCheckFlag = true;

        private static bool _alreadyKownNotNeedIntroduce;

        public static bool NeedIntroduce()
        {
            //short cut
            if (_alreadyKownNotNeedIntroduce)
            {
                return false;
            }

            bool introducedYet = PreferenceProvider.IsIntroducedYet;

            if (!introducedYet)
            {
                _firstCheckFlag = false;
                //没有介绍过
                return true;
            }

            //当前版本的第一次登陆的第一次检查
            bool isFirstLanch = VersionTracking.IsFirstLaunchForCurrentBuild && _firstCheckFlag;
            _firstCheckFlag = false;

            if (isFirstLanch)
            {
                PreferenceProvider.IsIntroducedYet = false;
                return true;
            }

            _alreadyKownNotNeedIntroduce = true;

            return false;
        }
    }
}