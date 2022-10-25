﻿#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HB.FullStack.Common
{
    /// <summary>
    /// 加入的任务，不能确定按顺序完成。
    /// </summary>
    public class WeakAsyncEventManager
    {
        private readonly Dictionary<string, List<DelegateWrapper>> _delegateWrapperDict = new Dictionary<string, List<DelegateWrapper>>();

        public void Add<TSender, TEventArgs>(Func<TSender, TEventArgs, Task> handlerDelegate, [CallerMemberName] string eventName = "") where TSender : class where TEventArgs : class
        {
            WeakAsyncEventManagerExecutor.Add(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Add<TSender>(Func<TSender, object, Task> handlerDelegate, [CallerMemberName] string eventName = "") where TSender : class
        {
            WeakAsyncEventManagerExecutor.Add(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Add(Func<object, object,Task> handlerDelegate, [CallerMemberName] string eventName = "")
        {
            WeakAsyncEventManagerExecutor.Add(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Remove<TSender, TEventArgs>(Func<TSender, TEventArgs, Task> handlerDelegate, [CallerMemberName] string eventName = "") where TSender : class where TEventArgs : class
        {
            WeakAsyncEventManagerExecutor.Remove(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Remove<TSender>(Func<TSender, object, Task> handlerDelegate, [CallerMemberName] string eventName = "") where TSender : class
        {
            WeakAsyncEventManagerExecutor.Remove(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Remove(Func<object, object, Task> handlerDelegate, [CallerMemberName] string eventName = "")
        {
            WeakAsyncEventManagerExecutor.Remove(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public Task RaiseEventAsync<TSender, TEventArgs>(string eventName, TSender sender, TEventArgs eventArgs) where TSender : class where TEventArgs : class
        {
            return WeakAsyncEventManagerExecutor.RaiseEventAsync(eventName, sender, eventArgs, _delegateWrapperDict);
        }

        public Task RaiseEventAsync<TSender>(string eventName, TSender sender, object eventArgs) where TSender : class
        {
            return WeakAsyncEventManagerExecutor.RaiseEventAsync(eventName, sender, eventArgs, _delegateWrapperDict);
        }

        public Task RaiseEventAsync(string eventName, object sender, object eventArgs)
        {
            return WeakAsyncEventManagerExecutor.RaiseEventAsync(eventName, sender, eventArgs, _delegateWrapperDict);
        }
    }


}
#nullable restore
