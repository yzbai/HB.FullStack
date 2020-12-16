#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace HB.FullStack.Common
{
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

        internal static Task RaiseEventAsync<TSender, TEventArgs>(string eventName, TSender sender, TEventArgs eventArgs, Dictionary<string, List<DelegateWrapper>> delegateWrapperDict) where TSender : class where TEventArgs : class
        {
            if (!delegateWrapperDict.TryGetValue(eventName, out List<DelegateWrapper> wrappers))
            {
                return Task.CompletedTask;
            }

            List<DelegateWrapper> toRemoves = new List<DelegateWrapper>();
            List<(object, MethodInfo)> toRaises = new List<(object, MethodInfo)>();

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

            List<Task> tasks = new List<Task>();

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

                tasks.Add(task);
            }

            return Task.WhenAll(tasks);
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