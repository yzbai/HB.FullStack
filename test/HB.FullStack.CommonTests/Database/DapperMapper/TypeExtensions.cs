#nullable disable
using System;
using System.Reflection;

namespace ClassLibrary1
{
    internal static class TypeExtensions
    {
        public static MethodInfo GetPublicInstanceMethod(this Type type, string name, Type[] types)
            => type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, types, null);
    }
}
#nullable restore