/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using HB.FullStack.Common;

namespace HB.FullStack.Client.MauiLib.Base
{
    //TODO: 是否使用Toolkit中的ObservableObject
    public interface IBaseViewModel : INotifyPropertyChanging, INotifyPropertyChanged
    {
        //bool IsBusy { get; }

        //string Title { get; }

        /// <summary>
        /// 本质是：像网络请求一样，为无状态 储存或恢复状态
        /// ViewModel可以是Singleton，所以多个page先后使用同一个viewmodel，这里做准备工作。与这个page相关的数据被建立
        /// </summary>
        Task OnPageAppearingAsync();

        /// <summary>
        /// page将要消失时执行，viewmodel做清理工作。与这个page相关的数据被清楚
        /// </summary>
        Task OnPageDisappearingAsync();

        void OnException(Exception ex, string message, ExceptionHandler handler, bool report = false, [CallerMemberName] string caller = "");

        void OnExceptionDisplay(Exception ex, string message, ExceptionDisplayMode displayMode = ExceptionDisplayMode.Toast, bool report = false, [CallerMemberName] string caller = "");
    }
}