﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Common.Files;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Client.MauiLib.Components
{
    public class SmsVerifyViewModel : BaseViewModel
    {
        public SmsVerifyViewModel(ILogger logger, ITokenPreferences clientPreferences, IFileManager fileManager) : base(logger, clientPreferences, fileManager)
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