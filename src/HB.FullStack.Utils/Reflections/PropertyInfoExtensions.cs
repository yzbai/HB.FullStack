
using System;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class PropertyInfoExtensions
    {
        private static readonly Type _isExternalInitType = typeof(System.Runtime.CompilerServices.IsExternalInit);

        public static bool IsInitOnlyOrNoSetMethod(this PropertyInfo propertyInfo)
        {
            MethodInfo? setMethod = propertyInfo.SetMethod;

            if (setMethod == null)
            {
                return true;
            }

            return setMethod.ReturnParameter.GetRequiredCustomModifiers().Contains(_isExternalInitType);
        }

        //public static MethodInfo? GetSetterMethod(this PropertyInfo propertyInfo, Type type)
        //{
        //    if (propertyInfo.DeclaringType == type) return propertyInfo.GetSetMethod(true);

        //    return propertyInfo.DeclaringType?.GetProperty(
        //           propertyInfo.Name,
        //           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
        //           Type.DefaultBinder,
        //           propertyInfo.PropertyType,
        //           propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray(),
        //           null)?.GetSetMethod(true);
        //}

        //public static MethodInfo? GetGetterMethod(this PropertyInfo propertyInfo, Type type)
        //{
        //    if (propertyInfo.DeclaringType == type)
        //    {
        //        return propertyInfo.GetGetMethod(true);
        //    }

        //    return propertyInfo.DeclaringType?.GetProperty(
        //           propertyInfo.Name,
        //           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
        //           Type.DefaultBinder,
        //           propertyInfo.PropertyType,
        //           propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray(),
        //           null)?.GetGetMethod(true);
        //}



    }
}