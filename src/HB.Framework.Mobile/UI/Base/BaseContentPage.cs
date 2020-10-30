using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using HB.Framework.Client.Services;
using HB.Framework.Client.UI.Platform;
using Xamarin.Forms;

namespace HB.Framework.Client.Base
{
    public abstract class BaseContentPage : ContentPage
    {
        private bool _showNavigationPageNavigationBar = true;

        public bool IsAppearing { get; private set; }

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
                return DependencyService.Resolve<IStatusBar>().IsShowing;
            }
            set
            {
                if (value)
                {
                    DependencyService.Resolve<IStatusBar>().Show();
                }
                else
                {
                    DependencyService.Resolve<IStatusBar>().Hide();
                }
            }
        }

        public bool DisableBackButton { get; set; }

        public BaseContentPage()
        {
            ControlTemplate = (ControlTemplate)Application.Current.Resources["BaseContentPageControlTemplate"];
        }

        protected abstract IList<IBaseContentView?>? GetAllCustomerControls();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            IsAppearing = true;

            IList<IBaseContentView?>? contentViews = GetAllCustomerControls();

            if (contentViews == null)
            {
                return;
            }

            foreach (IBaseContentView? baseContentView in contentViews)
            {
                baseContentView?.OnAppearing();
            }

            if (BindingContext is BaseViewModel viewModel)
            {
                viewModel.OnAppearing();
            }

            ExecuteAppearedAsync().Fire();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            IsAppearing = false;

            IList<IBaseContentView?>? contentViews = GetAllCustomerControls();

            if (contentViews == null)
            {
                return;
            }

            foreach (IBaseContentView? baseContentView in contentViews)
            {
                baseContentView?.OnDisappearing();
            }

            if (BindingContext is BaseViewModel viewModel)
            {
                viewModel.OnDisappearing();
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