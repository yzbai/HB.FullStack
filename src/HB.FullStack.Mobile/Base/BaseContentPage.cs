using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HB.FullStack.Client.Platforms;
using Xamarin.Forms;

namespace HB.FullStack.Client.Base
{
    public abstract class BaseContentPage : ContentPage
    {
        private bool _showNavigationPageNavigationBar = true;

        public bool IsAppearing { get; private set; }

        public string PageName { get; private set; }

        public bool ShowShellTabBar { get => Shell.GetTabBarIsVisible(this); set => Shell.SetTabBarIsVisible(this, value); }

        public bool ShowShellNavBar { get => Shell.GetNavBarIsVisible(this); set => Shell.SetNavBarIsVisible(this, value); }

        public bool ShowShellNavBarShadow { get => Shell.GetNavBarHasShadow(this); set => Shell.SetNavBarHasShadow(this, value); }

        public bool ShowNavigationPageNavigationBar
        {
            get => _showNavigationPageNavigationBar;
            set
            {
                _showNavigationPageNavigationBar = value;
                NavigationPage.SetHasNavigationBar(this, value);
            }
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public bool ShowStatusBar
        {
            get
            {
                return DependencyService.Resolve<IPlatformStatusBarHelper>().IsShowing;
            }
            set
            {
                if (value)
                {
                    DependencyService.Resolve<IPlatformStatusBarHelper>().Show();
                }
                else
                {
                    DependencyService.Resolve<IPlatformStatusBarHelper>().Hide();
                }
            }
        }

        public bool DisableBackButton { get; set; }

        public BaseContentPage()
        {
            ControlTemplate = (ControlTemplate)Application.Current.Resources["BaseContentPageControlTemplate"];
            PageName = GetType().Name;

            Application.Current.LogUsage(UsageType.PageCreate, PageName);
        }

        protected abstract IList<IBaseContentView?>? GetAllCustomerControls();

        protected override void OnAppearing()
        {
            Application.Current.LogUsage(UsageType.PageAppearing, PageName);

            base.OnAppearing();

            IsAppearing = true;

            //baseContentViews
            GetAllCustomerControls().ForEach(v => v?.OnAppearing());

            //viewmodel
            if (BindingContext is BaseViewModel viewModel)
            {
                viewModel.OnAppearing(PageName);
            }

            ExecuteAppearedAsync().Fire();
        }

        protected override void OnDisappearing()
        {
            Application.Current.LogUsage(UsageType.PageDisappearing, PageName);

            base.OnDisappearing();

            IsAppearing = false;

            //baseContentViews
            GetAllCustomerControls().ForEach(v => v?.OnDisappearing());

            //viewmodel
            if (BindingContext is BaseViewModel viewModel)
            {
                viewModel.OnDisappearing(PageName);
            }
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