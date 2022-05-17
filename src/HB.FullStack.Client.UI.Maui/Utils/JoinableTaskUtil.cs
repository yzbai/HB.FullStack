using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.VisualStudio.Threading;

namespace System
{
    public static class JoinableTaskUtil
    {
        public static JoinableTaskFactory JoinableTaskFactory { get; } = new JoinableTaskFactory(JoinableTaskContext);

        //https://github.com/Microsoft/vs-threading/blob/main/doc/library_with_jtf.md#singleton
        private static JoinableTaskContext? _joinableTaskContext;

        /// <summary>
        /// Gets or sets the JoinableTaskContext created on the main thread of the application hosting this library.
        /// </summary>
        public static JoinableTaskContext JoinableTaskContext
        {
            get
            {
                if (_joinableTaskContext is null)
                {
                    // This self-initializer is for when an app does not have a `JoinableTaskContext` to pass to the library.
                    // Our private instance will only work if this property getter first runs on the main thread of the application
                    // since creating a JoinableTaskContext captures the thread and SynchronizationContext.

                    //TODO: 测试这个，看是不是主线程在创建JoinableContext
                    if (MainThread.IsMainThread)
                    {
                        _joinableTaskContext = new JoinableTaskContext();
                    }
                    else
                    {
                        //TODO: 测试这个
                        MainThread.InvokeOnMainThreadAsync(() => { _joinableTaskContext = new JoinableTaskContext(); }).Wait();
                    }
                }

                //while (_joinableTaskContext == null)
                //{
                //}

                return _joinableTaskContext!;
            }

            set
            {
                Assumes.True(_joinableTaskContext is null || _joinableTaskContext == value, "This property has already been set to another value or is set after its value has been retrieved with a self-created value. Set this property once, before it is used elsewhere.");
                _joinableTaskContext = value;
            }
        }
    }
}