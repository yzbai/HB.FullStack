#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HB.Framework.Common
{
    public delegate Task AsyncEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs args);

    public delegate Task AsyncEventHandler(object sender, EventArgs args);

    public delegate Task AsyncEventHandler<TSender>(TSender sender, EventArgs args);

    internal class DelegateWrapper
    {
        public DelegateWrapper(WeakReference caller, MethodInfo handler)
        {
            CallerWeakReference = caller;
            Handler = handler;
        }

        public WeakReference CallerWeakReference { get; set; }

        public MethodInfo Handler { get; set; }
    }

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
    }

    static class WeakAsyncEventManagerExecutor
    {
        internal static void Add(string eventName, object caller, MethodInfo methodInfo, Dictionary<string, List<DelegateWrapper>> delegateWrapperDict)
        {
            if (!delegateWrapperDict.TryGetValue(eventName, out List<DelegateWrapper> wrappers))
            {
                wrappers = new List<DelegateWrapper>();
                delegateWrapperDict.Add(eventName, wrappers);
            }

            if (caller == null)
            {
                wrappers.Add(new DelegateWrapper(null, methodInfo));
            }
            else
            {
                wrappers.Add(new DelegateWrapper(new WeakReference(caller), methodInfo));
            }
        }

        internal static void Remove(string eventName, object caller, MethodInfo methodInfo, Dictionary<string, List<DelegateWrapper>> delegateWrapperDict)
        {
            if (!delegateWrapperDict.TryGetValue(eventName, out List<DelegateWrapper> wrappers))
            {
                return;
            }

            DelegateWrapper wrapper = wrappers.SingleOrDefault(w => w.CallerWeakReference?.Target == caller && w.Handler.Name == methodInfo.Name);

            if (wrapper != null)
            {
                wrappers.Remove(wrapper);
            }
        }

        internal static async Task RaiseEventAsync<TSender, TEventArgs>(string eventName, TSender sender, TEventArgs eventArgs, Dictionary<string, List<DelegateWrapper>> delegateWrapperDict) where TSender : class where TEventArgs : class
        {
            if (!delegateWrapperDict.TryGetValue(eventName, out List<DelegateWrapper> wrappers))
            {
                return;
            }

            List<DelegateWrapper> toRemoves = new();
            List<(object, MethodInfo)> toRaises = new();

            foreach (DelegateWrapper wrapper in wrappers)
            {
                object caller = wrapper.CallerWeakReference?.Target;

                if (wrapper.CallerWeakReference != null && caller == null)
                {
                    toRemoves.Add(wrapper);
                }
                else
                {
                    toRaises.Add((caller, wrapper.Handler));
                }
            }

            //clean
            toRemoves.ForEach(w => wrappers.Remove(w));

            //Invoke
            for (int i = 0; i < toRaises.Count; ++i)
            {
                (object caller, MethodInfo methodInfo) = toRaises[i];

                object rtObj;

                if (methodInfo.IsLightweightMethod())
                {
                    DynamicMethod dynamicMethodInfo = TryGetDynamicMethod(methodInfo);
                    rtObj = dynamicMethodInfo?.Invoke(caller, new object[] { sender, eventArgs });
                }
                else
                {
                    rtObj = methodInfo.Invoke(caller, new object[] { sender, eventArgs });
                }

                Task task = (Task)rtObj;

                await task.ConfigureAwait(false);
            }
        }

        static DynamicMethod TryGetDynamicMethod(in MethodInfo rtDynamicMethod)
        {
            var typeInfoRTDynamicMethod = typeof(DynamicMethod).GetTypeInfo().GetDeclaredNestedType("RTDynamicMethod");
            var typeRTDynamicMethod = typeInfoRTDynamicMethod?.AsType();

            return (typeInfoRTDynamicMethod?.IsAssignableFrom(rtDynamicMethod.GetType().GetTypeInfo()) ?? false) ?
                 (DynamicMethod)typeRTDynamicMethod.GetRuntimeFields().First(f => f.Name is "m_owner").GetValue(rtDynamicMethod)
                : null;
        }

        static bool IsLightweightMethod(this MethodBase method)
        {
            var typeInfoRTDynamicMethod = typeof(DynamicMethod).GetTypeInfo().GetDeclaredNestedType("RTDynamicMethod");
            return method is DynamicMethod || (typeInfoRTDynamicMethod?.IsAssignableFrom(method.GetType().GetTypeInfo()) ?? false);
        }
    }


}
#nullable restore