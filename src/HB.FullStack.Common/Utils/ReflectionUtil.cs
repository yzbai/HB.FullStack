
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace System
{
    public static class ReflectionUtil
    {
        public static bool IsValueTypeOrString(Type? propertyType)
        {
            return propertyType != null && (propertyType.IsValueType || propertyType == typeof(string));
        }

        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //return Directory
            //    .GetFiles(path, "*.dll")
            //    .Select(f => Assembly.LoadFile(f));
        }

        public static IEnumerable<Type> GetAllTypeByCondition(Func<Type, bool> condition)
        {

            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(t => condition(t));

            //通过File的方式会导致Type的HashCode不一致
            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //return Directory
            //    .GetFiles(path, "*.dll")
            //    .SelectMany(file => Assembly.LoadFile(file).GetTypes())
            //    .Where(t => condition(t));
        }

        public static IEnumerable<Type> GetAllTypeByCondition(IList<string> assembliesToCheck, Func<Type, bool> condition)
        {
            return assembliesToCheck
                .SelectMany(assemblyName => Assembly.Load(assemblyName).GetTypes())
                .Where(t => condition(t));
        }

        public static ConstructorInfo? GetDefaultConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes);
        }

        public static MethodInfo? GetSetterMethod(this PropertyInfo propertyInfo, Type type)
        {
            if (propertyInfo.DeclaringType == type) return propertyInfo.GetSetMethod(true);

            return propertyInfo.DeclaringType?.GetProperty(
                   propertyInfo.Name,
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                   Type.DefaultBinder,
                   propertyInfo.PropertyType,
                   propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray(),
                   null)?.GetSetMethod(true);
        }

        public static MethodInfo? GetGetterMethod(this PropertyInfo propertyInfo, Type type)
        {
            if (propertyInfo.DeclaringType == type)
            {
                return propertyInfo.GetGetMethod(true);
            }

            return propertyInfo.DeclaringType?.GetProperty(
                   propertyInfo.Name,
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                   Type.DefaultBinder,
                   propertyInfo.PropertyType,
                   propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray(),
                   null)?.GetGetMethod(true);
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

        public static PropertyInfo? GetPropertyInfoByAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetProperties().Where(p => p.GetCustomAttribute<T>() != null).FirstOrDefault();
        }

        public static IList<PropertyInfo> GetPropertyInfosByAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetProperties().Where(p => p.GetCustomAttribute<T>() != null).ToList();
        }

    }
}