using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;


using HB.FullStack.Client.Maui.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.Maui.Base
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

        /// <summary>
        /// TODO: ViewModelOnPageAppearingTask应该在OnNavigatedTo方法里等待。但需要官方团队解决问题
        /// https://github.com/dotnet/maui/issues/7320
        /// </summary>
        private List<Task> _pendingTasks = new List<Task>();

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
            if (ViewModel != null)
            {
                _pendingTasks.Add(ViewModel.OnPageAppearingAsync());
            }

            //baseContentViews
            //TODO: 检查各个自定义控件的OnPageAppearing方法有没有改成异步的。
            if (CustomerControls != null)
            {
                Parallel.ForEach(CustomerControls, v => v.OnPageAppearing());
            }
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (_pendingTasks.Any())
            {
                Task.WaitAll(_pendingTasks.ToArray());
            }

            //TODO:是否需要在ViewModel中设置PageAppeared呢？
        }

        protected override async void OnDisappearing()
        {
            if (CustomerControls != null)
            {
                Parallel.ForEach(CustomerControls, v => v.OnPageDisappearing());
            }

            if (ViewModel != null)
            {
                await ViewModel.OnPageDisappearingAsync().ConfigureAwait(false);
            }

            base.OnDisappearing();
        }

        public IList<IBaseContentView>? CustomerControls { get; protected set; }

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