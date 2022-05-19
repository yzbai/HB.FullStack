using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;


using HB.FullStack.Client.UI.Maui.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.UI.Maui.Base
{
    public abstract class BaseContentPage<TViewModel> : BaseContentPage where TViewModel : BaseViewModel
    {
        protected BaseContentPage(TViewModel viewModel) : base(viewModel)
        {
        }

        public new TViewModel ViewModel => (TViewModel)base.ViewModel!;
    }

    public abstract class BaseContentPage : ContentPage
    {
        public BaseViewModel? ViewModel { get; protected set; }

        public Task? ViewModelOnPageAppearingTask { get; set; }

        protected BaseContentPage(BaseViewModel? viewModel)
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue("BaseContentPageControlTemplate", out object controlTemplate))
            {
                ControlTemplate = (ControlTemplate)controlTemplate;
            }

            BindingContext = ViewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //viewmodel
            ViewModelOnPageAppearingTask = ViewModel?.OnPageAppearingAsync();

            //baseContentViews
            foreach (IBaseContentView v in GetAllCustomerControls())
            {
                v.OnPageAppearing();
            }

            Parallel.ForEach(GetAllCustomerControls(), v => v.OnPageAppearing());
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            Parallel.ForEach(GetAllCustomerControls(), v => v.OnPageDisappearing());

            if (ViewModel != null)
            {
                await ViewModel.OnPageDisappearingAsync().ConfigureAwait(false);

            }
        }

        protected abstract IList<IBaseContentView> GetAllCustomerControls();

        #region Back Button

        public bool DisableBackButton { get; set; }

        protected override bool OnBackButtonPressed()
        {
            if (DisableBackButton)
            {
                return true;
            }

            return base.OnBackButtonPressed();
        }

        #endregion

        #region Visual Settings

        public bool IsNavBarVisible
        {
            get
            {
                if (Application.Current?.MainPage is Shell)
                {
                    return Shell.GetNavBarIsVisible(this);
                }
                else if (Application.Current?.MainPage is NavigationPage)
                {
                    return NavigationPage.GetHasNavigationBar(this);
                }

                return false;
            }
            set
            {
                if (Application.Current?.MainPage is Shell)
                {
                    Shell.SetNavBarIsVisible(this, value);
                }
                else if (Application.Current?.MainPage is NavigationPage)
                {
                    NavigationPage.SetHasNavigationBar(this, value);
                }
            }
        }

        public bool IsBottomTabBarVisible
        {
            get
            {
                if (Application.Current?.MainPage is Shell)
                {
                    return Shell.GetTabBarIsVisible(this);
                }

                return false;
            }
            set
            {
                if (Application.Current?.MainPage is Shell)
                {
                    Shell.SetTabBarIsVisible(this, value);
                }
            }
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public bool IsFullScreen
        {
            get
            {
                return PlatformUtil.IsFullScreen;
            }
            set
            {
                PlatformUtil.IsFullScreen = value;
            }
        }

        #endregion
    }
}