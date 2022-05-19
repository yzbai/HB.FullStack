using System;
using System.Threading.Tasks;

using HB.FullStack.Common;

namespace HB.FullStack.Client
{
    //TODO: 是否使用Toolkit中的ObservableObject
    public abstract class BaseViewModel : ObservableObject
    {
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
    }
}