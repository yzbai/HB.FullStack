using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Common.Files;

using Microsoft.Extensions.Logging;

namespace Todo.Client.MobileApp.ViewModels
{
    internal partial class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(ILogger<HomeViewModel> logger, ITokenPreferences preferenceProvider, IFileManager fileManager) : base(logger, preferenceProvider, fileManager)
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

        [RelayCommand]
        private Task LogoutAsync()
        {
            TokenPreferences.
        }
    }
}
