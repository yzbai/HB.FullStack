using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.Components.Users;
using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Common.Files;

using Microsoft.Extensions.Logging;

namespace Todo.Client.MobileApp.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public HomeViewModel(IUserService userService)
        {
            _userService = userService;
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
        private async Task LogoutAsync()
        {
            try
            {
                await _userService.LogoutAsync();

            }
            catch(Exception ex)
            {
                Currents.ShowToast(ex.Message);
            }

            await NavigationHelper.GotoLoginPageAsync();
        }
    }
}
