using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Client.Maui.Base
{
    public abstract partial class BaseViewModel : ObservableObject, IBaseViewModel
    {
        public const string ExceptionDisplaySignalName = "HB.FullStack.Client.Maui.ExceptionDisplay";
        public ILogger Logger { get; }

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        protected BaseViewModel(ILogger? logger = null)
        {
            Logger = logger ?? GlobalSettings.Logger;
        }

        /// <summary>
        /// ViewModel可以是Singleton，所以多个page先后使用同一个viewmodel，这里做准备工作。与这个page相关的数据被建立
        /// </summary>
        public abstract Task OnPageAppearingAsync();

        /// <summary>
        /// page将要消失时执行，viewmodel做清理工作。与这个page相关的数据被清楚
        /// </summary>
        public abstract Task OnPageDisappearingAsync();

        #region Exception Handle

        protected ExceptionDisplayMode ExceptionDisplayMode { get; set; } = ExceptionDisplayMode.Toast;

        public virtual void OnException(Exception ex, string message, ExceptionHandler handler, bool report = false, [CallerMemberName] string caller = "")
        {
            handler(ex, message, caller);
        }

        public virtual void OnException(Exception ex, string message, ExceptionDisplayMode displayMode = ExceptionDisplayMode.Toast, bool report = false, [CallerMemberName] string caller = "")
        {
            Logger.LogError(ex, message);

            //TODO:错误上报处理report
            //TODO: 处理displayMode

            Currents.Page.DisplayAlert("Exception", message, "OK")
                .SafeFireAndForget(ex => { GlobalSettings.Logger.LogCritical(ex, "ViewModel的OnException显示Alert挂了."); });
        }

        #endregion

        #region UIs

        public static void ShowToast(string message, ToastDuration duration = ToastDuration.Long, double textSize = 14) => Currents.ShowToast(message, duration, textSize);

        #endregion
    }
}