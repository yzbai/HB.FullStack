#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HB.FullStack.Common
{
    /// <summary>
    /// 加入的任务，会并发执行，不会按顺序执行
    /// </summary>
    public class WeakAsyncEventManager
    {
        private readonly Dictionary<string, List<DelegateWrapper>> _delegateWrapperDict = new Dictionary<string, List<DelegateWrapper>>();

        public void Add<TSender, TEventArgs>(AsyncEventHandler<TSender, TEventArgs> handlerDelegate, [CallerMemberName] string eventName = "") where TSender : class where TEventArgs : class
        {
            WeakAsyncEventManagerExecutor.Add(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Add<TSender>(AsyncEventHandler<TSender> handlerDelegate, [CallerMemberName] string eventName = "") where TSender : class
        {
            WeakAsyncEventManagerExecutor.Add(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Add(AsyncEventHandler handlerDelegate, [CallerMemberName] string eventName = "")
        {
            WeakAsyncEventManagerExecutor.Add(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Remove<TSender, TEventArgs>(AsyncEventHandler<TSender, TEventArgs> handlerDelegate, [CallerMemberName] string eventName = "") where TSender : class where TEventArgs : class
        {
            WeakAsyncEventManagerExecutor.Remove(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Remove<TSender>(AsyncEventHandler<TSender> handlerDelegate, [CallerMemberName] string eventName = "") where TSender : class
        {
            WeakAsyncEventManagerExecutor.Remove(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

        public void Remove(AsyncEventHandler handlerDelegate, [CallerMemberName] string eventName = "")
        {
            WeakAsyncEventManagerExecutor.Remove(eventName, handlerDelegate.Target, handlerDelegate.Method, _delegateWrapperDict);
        }

#pragma warning disable CA1030 // Use events where appropriate
        public Task RaiseEventAsync<TSender, TEventArgs>(string eventName, TSender sender, TEventArgs eventArgs) where TSender : class where TEventArgs : class
        {
            return WeakAsyncEventManagerExecutor.RaiseEventAsync<TSender, TEventArgs>(eventName, sender, eventArgs, _delegateWrapperDict);
        }

        public Task RaiseEventAsync<TSender>(string eventName, TSender sender, EventArgs eventArgs) where TSender : class
        {
            return WeakAsyncEventManagerExecutor.RaiseEventAsync<TSender, EventArgs>(eventName, sender, eventArgs, _delegateWrapperDict);
        }

        public Task RaiseEventAsync(string eventName, object sender, EventArgs eventArgs)
        {
            return WeakAsyncEventManagerExecutor.RaiseEventAsync<object, EventArgs>(eventName, sender, eventArgs, _delegateWrapperDict);
        }
#pragma warning restore CA1030 // Use events where appropriate
    }


}
#nullable restore