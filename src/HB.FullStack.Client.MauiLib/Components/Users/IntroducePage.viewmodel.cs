﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading.Tasks;
using HB.FullStack.Common.Files;
using HB.FullStack.Client.MauiLib.Base;

using Microsoft.Extensions.Logging;
using HB.FullStack.Client.Abstractions;

namespace HB.FullStack.Client.MauiLib.Components
{
    public class IntroduceViewModel : BaseViewModel
    {
        public IntroduceViewModel(ILogger logger, ITokenPreferences preferenceProvider, IFileManager fileManager) : base(logger, preferenceProvider, fileManager)
        {
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