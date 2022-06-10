using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using HB.FullStack.XamarinForms.Platforms;

using Microsoft.Extensions.Logging;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Base
{
    public abstract class BaseContentPage : ContentPage
    {
        public bool IsAppearing { get; private set; }

        public string PageTypeName { get; private set; }

        //public bool NeedLogined { get; set; }

        public bool NavBarIsVisible
        {
            get
            {
                if (Application.Current.MainPage is Shell)
                {
                    return Shell.GetNavBarIsVisible(this);
                }
                else if (Application.Current.MainPage is NavigationPage)
                {
                    return NavigationPage.GetHasNavigationBar(this);
                }

                return false;
            }
            set
            {
                if (Application.Current.MainPage is Shell)
                {
                    Shell.SetNavBarIsVisible(this, value);
                }
                else if (Application.Current.MainPage is NavigationPage)
                {
                    NavigationPage.SetHasNavigationBar(this, value);
                }
            }
        }

        public bool BottomTabBarIsVisible
        {
            get
            {
                if (Application.Current.MainPage is Shell)
                {
                    return Shell.GetTabBarIsVisible(this);
                }

                return false;
            }
            set
            {
                if (Application.Current.MainPage is Shell)
                {
                    Shell.SetTabBarIsVisible(this, value);
                }
            }
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public bool StatusBarIsVisible
        {
            get
            {
                return PlatformHelper.Current.IsStatusBarShowing;
            }
            set
            {
                if (value)
                {
                    PlatformHelper.Current.ShowStatusBar();
                }
                else
                {
                    PlatformHelper.Current.HideStatusBar();
                }
            }
        }

        public bool DisableBackButton { get; set; }

        protected BaseContentPage()
        {
            if (Application.Current.Resources.TryGetValue("BaseContentPageControlTemplate", out object controlTemplate))
            {
                ControlTemplate = (ControlTemplate)controlTemplate;
            }

            PageTypeName = GetType().Name;

            //Application.Current.LogUsage(UsageType.PageCreate, PageName);
        }

        protected abstract IList<IBaseContentView?>? GetAllCustomerControls();

        protected override async void OnAppearing()
        {
            // 放到AppShell中去集中控制路由
            //if(NeedLogined && !UserPreferences.IsLogined)
            //{
            //    INavigationService.Current.PushLoginPage(false);
            //    BaseApplication.NavigationService.PushLoginPage(false);
            //    NavigationService.Current.PushLoginPage(false);
            //    return;
            //}

            base.OnAppearing();

            IsAppearing = true;
            
            //viewmodel
            if (BindingContext is BaseViewModel viewModel)
            {
                await viewModel.OnAppearingAsync(PageTypeName).ConfigureAwait(false);
            }

            //baseContentViews
            IList<IBaseContentView?>? customerControls = GetAllCustomerControls();

            if (customerControls != null)
            {
                foreach (IBaseContentView? v in customerControls)
                {
                    if (v == null)
                    {
                        GlobalSettings.Logger.LogDebug("######################   Shit happend!");
                    }

                    v?.OnAppearing();
                }
            }



            await ExecuteAppearedAsync().ConfigureAwait(false);
        }

        private async Task ExecuteAppearedAsync()
        {
            //TODO: This is a bullshit
            await Task.Delay(600).ConfigureAwait(true);

            await OnAppearedAsync().ConfigureAwait(true);
        }

        protected virtual Task OnAppearedAsync()
        {
            return Task.CompletedTask;
        }

        protected override async void OnDisappearing()
        {
            //Application.Current.LogUsage(UsageType.PageDisappearing, PageName);

            base.OnDisappearing();

            IsAppearing = false;

            //baseContentViews
            IList<IBaseContentView?>? customerControls = GetAllCustomerControls();

            if (customerControls != null)
            {
                foreach (var v in customerControls)
                {
                    v?.OnDisappearing();
                }
            }

            //viewmodel
            if (BindingContext is BaseViewModel viewModel)
            {
                await viewModel.OnDisappearingAsync(PageTypeName).ConfigureAwait(false);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (DisableBackButton)
            {
                return true;
            }

            return base.OnBackButtonPressed();
        }
    }
}