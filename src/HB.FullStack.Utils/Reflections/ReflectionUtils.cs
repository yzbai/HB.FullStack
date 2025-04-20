using System.Reflection;

namespace System
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// 获取类型转换MethodInfo，包括隐式或者显式
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static MethodInfo? GetConversionMethodInfo(Type from, Type to)
        {
            if (to == null) return null;
            MethodInfo[] fromMethods, toMethods;
            return ResolveOperator(fromMethods = from.GetMethods(BindingFlags.Static | BindingFlags.Public), from, to, "op_Implicit")
                ?? ResolveOperator(toMethods = to.GetMethods(BindingFlags.Static | BindingFlags.Public), from, to, "op_Implicit")
                ?? ResolveOperator(fromMethods, from, to, "op_Explicit")
                ?? ResolveOperator(toMethods, from, to, "op_Explicit");

            static MethodInfo? ResolveOperator(MethodInfo[] methods, Type from, Type to, string name)
            {
                for (int i = 0; i < methods.Length; i++)
                {
                    if (methods[i].Name != name || methods[i].ReturnType != to) continue;
                    var args = methods[i].GetParameters();
                    if (args.Length != 1 || args[0].ParameterType != from) continue;
                    return methods[i];
                }

                return null;
            }
        }
    }
}
