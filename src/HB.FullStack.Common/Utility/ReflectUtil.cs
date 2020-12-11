#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System
{
    //TODO:[Obsolete("请使用AssemblyLoadContext来实现", true)]
    public static class ReflectUtil
    {
        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return Directory
                .GetFiles(path, "*.dll")
                .Select(f => Assembly.LoadFile(f));
        }

        public static IEnumerable<Type> GetAllTypeByCondition(Func<Type, bool> condition)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return Directory
                .GetFiles(path, "*.dll")
                .SelectMany(file => Assembly.LoadFile(file).GetTypes())
                .Where(t => condition(t));
        }

        public static IEnumerable<Type> GetAllTypeByCondition(IList<string> assembliesToCheck, Func<Type, bool> condition)
        {
            return assembliesToCheck
                .SelectMany(assemblyName => Assembly.Load(assemblyName).GetTypes())
                .Where(t => condition(t));
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes);
        }

        public static MethodInfo GetPropertySetterMethod(PropertyInfo propertyInfo, Type type)
        {
            if (propertyInfo.DeclaringType == type) return propertyInfo.GetSetMethod(true);

            return propertyInfo.DeclaringType.GetProperty(
                   propertyInfo.Name,
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                   Type.DefaultBinder,
                   propertyInfo.PropertyType,
                   propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray(),
                   null).GetSetMethod(true);
        }

        public static MethodInfo GetPropertyGetterMethod(PropertyInfo propertyInfo, Type type)
        {
            if (propertyInfo.DeclaringType == type) return propertyInfo.GetGetMethod(true);

            return propertyInfo.DeclaringType.GetProperty(
                   propertyInfo.Name,
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                   Type.DefaultBinder,
                   propertyInfo.PropertyType,
                   propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray(),
                   null).GetGetMethod(true);
        }
    }
}