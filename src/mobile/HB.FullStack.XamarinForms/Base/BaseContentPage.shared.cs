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
        public bool IsAppearing { get; private set; }

        public string PageName { get; private set; }

        public bool NeedLogined { get; set; }

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
            if(NeedLogined && !UserPreferences.IsLogined)
            {
                NavigationService.Current.PushLoginPage(false);
                return;
            }

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
    }
}