using System;

namespace HB.Framework.Common
{
    public static class DelegateUtility
    {
        public static T Cast<T>(Delegate source) where T : class
        {
            T? rt = Cast(source, typeof(T)) as T;

            return rt.ThrowIfNull(nameof(rt));
        }

        /// <summary>
        /// Cast
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="MissingMethodException">Ignore.</exception>
        /// <exception cref="MethodAccessException">Ignore.</exception>
        public static Delegate Cast(Delegate source, Type type)
        {
            ThrowIf.Null(source, nameof(source));

            Delegate[] delegates = source.GetInvocationList();

            if (delegates.Length == 1)
            {
                return Delegate.CreateDelegate(type, delegates[0].Target, delegates[0].Method);
            }

            Delegate[] delegatesDest = new Delegate[delegates.Length];

            for (int nDelegate = 0; nDelegate < delegates.Length; nDelegate++)
            {
                delegatesDest[nDelegate] = Delegate.CreateDelegate(type, delegates[nDelegate].Target, delegates[nDelegate].Method);
            }

            return Delegate.Combine(delegatesDest);
        }
    }


}