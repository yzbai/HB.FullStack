/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Base
{
    public abstract class BasePage<TViewModel> : BasePage where TViewModel : BaseViewModel
    {
        protected BasePage() : base(Currents.Services.GetRequiredService<TViewModel>())
        {
        }

        public new TViewModel ViewModel => (TViewModel)base.ViewModel!;
    }

    public abstract class BasePage : ContentPage
    {
        public BaseViewModel? ViewModel { get; protected set; }

        //TODO: 使用SourceGeneration代替
        public IList<IBaseContentView> CustomerControls { get; } = new List<IBaseContentView>();

        protected abstract void RegisterCustomerControls(IList<IBaseContentView> customerControls);

        //private List<Task> _pendingTasks = new List<Task>();

        protected BasePage(BaseViewModel? viewModel)
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue("BaseContentPageControlTemplate", out object controlTemplate))
            {
                ControlTemplate = (ControlTemplate)controlTemplate;
            }

            BindingContext = ViewModel = viewModel;

            this.SetBinding(TitleProperty, nameof(BaseViewModel.Title));

            Loaded += BasePage_Loaded;

            Unloaded += BasePage_Unloaded;
        }

        protected virtual void BasePage_Loaded(object? sender, EventArgs e)
        {
            //TODO: Do we need ViewModel.OnPageLoaded()?
            //TODO: Do we need View.OnPageLoaded()?
        }

        protected virtual void BasePage_Unloaded(object? sender, EventArgs e)
        {
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            RegisterCustomerControls(CustomerControls);

            //viewmodel
            ViewModel?.OnPageAppearingAsync().SafeFireAndForget();
            //_pendingTasks.Add(ViewModel?.OnPageAppearingAsync()??Task.CompletedTask);

            //baseContentViews
            
            if (CustomerControls.IsNotNullOrEmpty())
            {
                Parallel.ForEach(CustomerControls, controls => controls.OnPageAppearing());

                //foreach(var control in CustomerControls)
                //{
                //    control.OnPageAppearing();
                //}
            }
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            //TODO: ViewModelOnPageAppearingTask应该在OnNavigatedTo方法里等待。但需要官方团队解决问题
            // https://github.com/dotnet/maui/issues/7320
            base.OnNavigatedTo(args);

            //if (_pendingTasks.Any())
            //{
            //    Task.WaitAll(_pendingTasks.ToArray());
            //}

            //TODO:是否需要在ViewModel中设置PageAppeared呢？
        }

        protected override void OnDisappearing()
        {
            if (CustomerControls.IsNotNullOrEmpty())
            {
                Parallel.ForEach(CustomerControls, v => v.OnPageDisappearing());
            }

            ViewModel?.OnPageDisappearingAsync().SafeFireAndForget();

            base.OnDisappearing();
        }

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