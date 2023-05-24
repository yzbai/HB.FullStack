#nullable disable
using System;
using System.Reflection;

namespace HB.FullStack.BaseTest.DapperMapper
{
    internal static class TypeExtensions
    {
        public static MethodInfo GetPublicInstanceMethod(this Type type, string name, Type[] types)
            => type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, types, null);
    }
}
#nullable restore