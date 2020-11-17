#nullable enable

using System.Collections.Generic;
using System.Reflection;

namespace System
{
    public static class TypeGetMethodExtensions
    {
        public static IEnumerable<MethodInfo> GetMethodWithAttribute<T>(this Type type) where T : Attribute
        {
            List<MethodInfo> lst = new List<MethodInfo>();

            foreach (MethodInfo info in type.GetMethods())
            {
                if (info.GetCustomAttribute<T>() != null)
                {
                    lst.Add(info);
                }
            }

            return lst;
        }
    }
}