/*
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
using System.Collections;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using HB.FullStack.Client.MauiLib.Startup;

namespace HB.FullStack.Client.MauiLib.Components
{
    public partial class IntroduceViewModel : BaseViewModel
    {
        private readonly MauiOptions _options;

        [ObservableProperty]
        private IList<IntroduceContent>? _introduceContents;

        public IntroduceViewModel(ILogger<IntroduceViewModel> logger, ITokenPreferences preferenceProvider, IFileManager fileManager, IOptions<MauiOptions> options) : base(logger, preferenceProvider, fileManager)
        {
            _options = options.Value;

            IntroduceContents = _options.IntoduceContents;
        }

        public override Task OnPageAppearingAsync()
        {
            return Task.CompletedTask;
        }

        public override Task OnPageDisappearingAsync()
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task OnFinishedAsync()
        {
            Currents.IsIntroducedYet = true;

            await NavigationHelper.OnIntroduceFinishedAsync();
        }
    }
}