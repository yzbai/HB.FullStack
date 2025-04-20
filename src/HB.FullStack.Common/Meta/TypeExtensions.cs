using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class TypeExtensions
    {
        public static bool IsValueTypeOrString(Type? propertyType)
        {
            return propertyType != null && (propertyType.IsValueType || propertyType == typeof(string));
        }

        public static MethodInfo? GetGetterMethodByAttribute<T>(this Type type) where T : Attribute
        {
            PropertyInfo? propertyInfo = type.GetPropertyInfoByAttribute<T>();

            if (propertyInfo == null) return null;

            return propertyInfo.GetGetterMethod(type);
        }

        public static MethodInfo? GetSetterMethodByAttribute<T>(this Type type) where T : Attribute
        {
            PropertyInfo? propertyInfo = type.GetPropertyInfoByAttribute<T>();

            if (propertyInfo == null) return null;

            return propertyInfo.GetSetterMethod(type);
        }

        public static ConstructorInfo? GetDefaultConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes);
        }

        public static PropertyInfo? GetPropertyInfoByAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetProperties().Where(p => p.GetCustomAttribute<T>(true) != null).FirstOrDefault();
        }

        public static IList<PropertyInfo> GetPropertyInfosByAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetProperties().Where(p => p.GetCustomAttribute<T>(true) != null).ToList();
        }

    }
}