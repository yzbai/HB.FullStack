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
    }
}
