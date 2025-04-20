
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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

        #region Access Methods

        private static ConcurrentDictionary<string, Func<object, object?>> _propertyGetDelegateCache = new ConcurrentDictionary<string, Func<object, object?>>();

        public static Func<object, object?> GetGetDelegate(this PropertyInfo? property)
        {
            if (property == null)
            {
                return _ => null;
            }

            string key = $"{property.DeclaringType!.FullName}_{property.Name}";

        }

        public static Func<object, object?> CreateGetDelegateByIL(this PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod(true)!;

            DynamicMethod dm = new DynamicMethod(
                $"PropertyGetByIL_{propertyInfo.Name}_{Guid.NewGuid()}",
                typeof(object),
                new Type[] { typeof(object) },
                propertyInfo.DeclaringType!,
                true);

            ILGenerator il = dm.GetILGenerator();

            LocalBuilder objectLocal = il.DeclareLocal(typeof(object));

            if (!getMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stloc, objectLocal);
            }

            EmitUtils.EmitGetPropertyValue(il, objectLocal, propertyInfo);

            il.Emit(OpCodes.Ret);

            return (Func<object, object?>)dm.CreateDelegate(typeof(Func<object, object?>));
        }

        public static Action<object, object?> CreateSetDelegateByIL(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return (_, _) => { };
            }

            MethodInfo setMethod = propertyInfo.GetSetMethod(true)!;

            DynamicMethod dm = new DynamicMethod(
                $"PropertySetByIL_{propertyInfo.Name}_{Guid.NewGuid()}",
                null,
                new Type[] { typeof(object), typeof(object) },
                propertyInfo.DeclaringType!,
                true);

            ILGenerator il = dm.GetILGenerator();

            LocalBuilder objectLocal = il.DeclareLocal(typeof(object));
            LocalBuilder propertyValueLocal = il.DeclareLocal(typeof(object));

            if (!setMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stloc, objectLocal);
            }

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc, propertyValueLocal);

            EmitUtils.EmitSetPropertyValue(il, objectLocal, propertyValueLocal, propertyInfo);

            il.Emit(OpCodes.Ret);

            return (Action<object, object?>)dm.CreateDelegate(typeof(Action<object, object?>));
        }

        #endregion

    }
}