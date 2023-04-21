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

using HB.FullStack.Client.MauiLib.Controls;

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
            }

            if (NavigationHelper.NeedLogin(pageName))
            {
                await NavigationHelper.GotoLoginPageAsync();
            }

            return;
        }
    }
}