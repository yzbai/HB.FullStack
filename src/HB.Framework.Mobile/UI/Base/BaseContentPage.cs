using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace HB.Framework.Client.Base
{
    public abstract class BaseContentPage : ContentPage
    {
        public bool IsAppearing { get; set; }

        protected IClientGlobal ClientGlobal { get; set; }

        public BaseContentPage()
        {
            ControlTemplate = (ControlTemplate)Application.Current.Resources["BaseContentPageControlTemplate"];
            ClientGlobal = DependencyService.Resolve<IClientGlobal>();
        }

        protected abstract IList<IBaseContentView?>? GetAllCustomerControls();

        protected override void OnAppearing()
        {
            Shell.SetTabBarIsVisible(this, ShowTabBar());
            Shell.SetNavBarIsVisible(this, ShowNavigationBar());
            Shell.SetNavBarHasShadow(this, ShowNavigationBarShadow());

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

        protected virtual bool ShowTabBar()
        {
            return true;
        }

        protected virtual bool ShowNavigationBar()
        {
            return true;
        }

        protected virtual bool ShowNavigationBarShadow()
        {
            return false;
        }
    }
}