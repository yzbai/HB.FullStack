﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Client.MauiLib.Components;

using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Base
{
    public class BaseShell : Shell
    {
        public BaseShell()
        {
            NavigationHelper.RegisterRouting();
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
        }

        protected override async void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            string pageName = Currents.PageName;

            if (NavigationHelper.NeedIntroduce() && pageName != nameof(IntroducePage))
            {
                await NavigationHelper.GotoIntroducePageAsync();
                return;
            }

            if (NavigationHelper.NeedLogin(pageName))
            {
                await NavigationHelper.GotoLoginPageAsync();
                return;
            }

            return;
        }
    }
}