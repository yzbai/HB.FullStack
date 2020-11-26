#nullable disable

using System;
using System.Reflection;

namespace HB.FullStack.Common
{
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


}
#nullable restore