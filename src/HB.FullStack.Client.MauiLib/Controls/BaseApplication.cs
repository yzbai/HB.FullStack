using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Controls
{
    public abstract class BaseApplication : Application
    {
        //private readonly StatusManager _statusManager;

        public BaseApplication(/*StatusManager statusManager*/)
        {
            //_statusManager = statusManager;

            //_statusManager.OnAppConstructed();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState);

            return window;
        }

        #region Lifecycle

        //进入方式1：启动，从not running or destroyed
        protected override void OnStart()
        {
            base.OnStart();

            //_statusManager.OnAppStart();

        }

        //进入方式2：恢复，从stopped
        protected override void OnResume()
        {
            base.OnResume();

            //_statusManager.OnAppResume();
        }

        //退出
        protected override void OnSleep()
        {
            base.OnSleep();

            //_statusManager.OnAppSleep();
        }

        #endregion
    }
}