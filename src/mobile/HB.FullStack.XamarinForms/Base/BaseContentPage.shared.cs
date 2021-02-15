using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using HB.FullStack.XamarinForms.Platforms;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Base
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
                return DependencyService.Resolve<IStatusBarHelper>().IsShowing;
            }
            set
            {
                if (value)
                {
                    DependencyService.Resolve<IStatusBarHelper>().Show();
                }
                else
                {
                    DependencyService.Resolve<IStatusBarHelper>().Hide();
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

            PageName = GetType().Name;

            //Application.Current.LogUsage(UsageType.PageCreate, PageName);
        }

        protected abstract IList<IBaseContentView?>? GetAllCustomerControls();

        protected override void OnAppearing()
        {
            //Application.Current.LogUsage(UsageType.PageAppearing, PageName);

            base.OnAppearing();

            IsAppearing = true;

            //baseContentViews
            IList<IBaseContentView?>? customerControls = GetAllCustomerControls();

            if (customerControls != null)
            {
                foreach (var v in customerControls)
                {
                    v?.OnAppearing();
                }
            }

            //viewmodel
            if (BindingContext is BaseViewModel viewModel)
            {
                viewModel.OnAppearing(PageName);
            }

            ExecuteAppearedAsync().Fire();
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

        protected override void OnDisappearing()
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
                viewModel.OnDisappearing(PageName);
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

        protected static void Push(Page page)
        {
            Device.BeginInvokeOnMainThread(() => Shell.Current.Navigation.PushAsync(page));
        }

        protected static void Pop()
        {
            Device.BeginInvokeOnMainThread(() => Shell.Current.Navigation.PopAsync());
        }
    }
}