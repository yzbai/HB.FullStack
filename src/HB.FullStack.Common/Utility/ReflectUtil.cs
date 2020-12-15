#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace System
{
    //TODO:[Obsolete("请使用AssemblyLoadContext来实现", true)]
    public static class ReflectUtil
    {
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

        public static Func<object, object> CreateGetValueDeleagte(Type declareType, string propertyName)
        {
            // (object instance) => (object)((declaringType)instance).propertyName

            var param_instance = Expression.Parameter(typeof(Object));
            var body_objToType = Expression.Convert(param_instance, declareType);
            var body_getTypeProperty = Expression.Property(body_objToType, propertyName);
            var body_return = Expression.Convert(body_getTypeProperty, typeof(Object));
            return Expression.Lambda<Func<Object, Object>>(body_return, param_instance).Compile();
        }

        public static Action<object, object> CreateSetValueDelagate(PropertyInfo property)
        {
            // (object instance, object value) => 
            //     ((instanceType)instance).Set_XXX((propertyType)value)

            //声明方法需要的参数
            var param_instance = Expression.Parameter(typeof(object));
            var param_value = Expression.Parameter(typeof(object));

            var body_instance = Expression.Convert(param_instance, property.DeclaringType);
            var body_value = Expression.Convert(param_value, property.PropertyType);
            var body_call = Expression.Call(body_instance, property.GetSetMethod(), body_value);

            return Expression.Lambda<Action<object, object>>(body_call, param_instance, param_value).Compile();
        }

        public static Func<object, object> CreateGetterEmit(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            MethodInfo getMethod = property.GetGetMethod(true);

            DynamicMethod dm = new DynamicMethod("PropertyGetter", typeof(object),
                new Type[] { typeof(object) },
                property.DeclaringType, true);

            ILGenerator il = dm.GetILGenerator();

            if (!getMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, getMethod, null);
            }
            else
                il.EmitCall(OpCodes.Call, getMethod, null);

            if (property.PropertyType.IsValueType)
                il.Emit(OpCodes.Box, property.PropertyType);
            il.Emit(OpCodes.Ret);
            return (Func<object, object>)dm.CreateDelegate(typeof(Func<object, object>));
        }

        public static Action<object, object> CreatePropertySetter(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            MethodInfo setMethod = property.GetSetMethod(true);

            DynamicMethod dm = new DynamicMethod("PropertySetter", null,
                new Type[] { typeof(object), typeof(object) }, property.DeclaringType, true);

            ILGenerator il = dm.GetILGenerator();

            if (!setMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            il.Emit(OpCodes.Ldarg_1);

            EmitCastToReference(il, property.PropertyType);
            if (!setMethod.IsStatic && !property.DeclaringType.IsValueType)
            {
                il.EmitCall(OpCodes.Callvirt, setMethod, null);
            }
            else
                il.EmitCall(OpCodes.Call, setMethod, null);

            il.Emit(OpCodes.Ret);
            return (Action<object, object>)dm.CreateDelegate(typeof(Action<object, object>));
        }

        private static void EmitCastToReference(ILGenerator il, Type type)
        {
            if (type.IsValueType)
                il.Emit(OpCodes.Unbox_Any, type);
            else
                il.Emit(OpCodes.Castclass, type);
        }

    }
}