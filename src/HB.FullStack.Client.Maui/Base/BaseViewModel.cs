using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using HB.FullStack.Common;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace HB.FullStack.Client.Maui.Base
{
    public abstract class BaseViewModel : ObservableObject, IBaseViewModel
    {
        public const string ExceptionDisplaySignalName = "HB.FullStack.Client.Maui.ExceptionDisplay";
        public ILogger Logger { get; }

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _title = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        protected BaseViewModel(ILogger? logger = null)
        {
            Logger = logger ?? GlobalSettings.Logger;
        }

        /// <summary>
        /// ViewModel可以是Singleton，所以多个page先后使用同一个viewmodel，这里做准备工作。与这个page相关的数据被建立
        /// </summary>
        public virtual Task OnPageAppearingAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// page将要消失时执行，viewmodel做清理工作。与这个page相关的数据被清楚
        /// </summary>
        public virtual Task OnPageDisappearingAsync()
        {
            return Task.CompletedTask;
        }

        public IDispatcher Dispatcher => BaseApplication.CurrentDispatcher;

        #region Exception Handle

        protected ExceptionDisplayMode ExceptionDisplayMode { get; set; } = ExceptionDisplayMode.Toast;

        public virtual void OnException(Exception ex, string message, ExceptionHandler handler, [CallerMemberName] string caller = "")
        {
            handler(ex, message, caller);
        }

        public virtual void OnException(Exception ex, string message, ExceptionDisplayMode displayMode = ExceptionDisplayMode.Toast, [CallerMemberName] string caller = "")
        {
            Logger.LogError(ex, message);

            MessagingCenter.Send(this, ExceptionDisplaySignalName, new ExceptionDisplayArguments(message, displayMode));
        }

        #endregion
    }
}