using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class ThrowIf
    {
        public static T ThrowIfNull<T>(this T o, string paramName) where T : class
        {
            if (o == null)
                throw new ArgumentNullException(paramName);

            return o;
        }

        public static string ThrowIfNullOrEmpty(this string o, string paramName)
        {
            if(string.IsNullOrEmpty(o))
            {
                throw new ArgumentNullException(paramName);
            }

            return o;
        }

        public static string ThrowIfNotEqual(this string a, string b, string paramName)
        {
            if (( a == null && b != null) || !a.Equals(b, GlobalSettings.Comparison))
            {
                throw new ArgumentException("参数值应该一样", paramName);
            }

            return a;
        }
    }
}
