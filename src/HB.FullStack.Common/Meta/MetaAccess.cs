using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Convert;

namespace System
{
    public static partial class MetaAccess
    {
        #region Property Access

        public static Func<object, object?> CreatePropertyGetDeleagteByExpression(this Type declareType, string propertyName)
        {
            // (object instance) => (object)((declaringType)instance).propertyName

            var param_instance = Expression.Parameter(typeof(object));
            var body_objToType = Expression.Convert(param_instance, declareType);
            var body_getTypeProperty = Expression.Property(body_objToType, propertyName);
            var body_return = Expression.Convert(body_getTypeProperty, typeof(object));
            return Expression.Lambda<Func<object, object>>(body_return, param_instance).Compile();
        }

        public static Action<object, object?> CreatePropertySetDelagateByExpression(this PropertyInfo property)
        {
            // (object instance, object value) => 
            //     ((instanceType)instance).Set_XXX((propertyType)value)

            //声明方法需要的参数
            var param_instance = Expression.Parameter(typeof(object));
            var param_value = Expression.Parameter(typeof(object));

            var body_instance = Expression.Convert(param_instance, property.DeclaringType!);
            var body_value = Expression.Convert(param_value, property.PropertyType);
            var body_call = Expression.Call(body_instance, property.GetSetMethod()!, body_value);

            return Expression.Lambda<Action<object, object?>>(body_call, param_instance, param_value).Compile();
        }

        private static object? ReturnNull(object obj)
        {
            return null;
        }

        private static void DoNothing(object obj, object? value)
        {
            //Do Nothing
        }

        public static Func<object, object?> CreatePropertyGetDelegateByIL(this PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
            {
                return ReturnNull;
            }

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

            EmitUtil.EmitGetPropertyValue(il, objectLocal, propertyInfo);

            il.Emit(OpCodes.Ret);

            return (Func<object, object?>)dm.CreateDelegate(typeof(Func<object, object?>));
        }

        public static Action<object, object?> CreatePropertySetDelegateByIL(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return DoNothing;
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

            EmitUtil.EmitSetPropertyValue(il, objectLocal, propertyValueLocal, propertyInfo);

            il.Emit(OpCodes.Ret);

            return (Action<object, object?>)dm.CreateDelegate(typeof(Action<object, object?>));
        }

        #endregion

        #region Convert Object

        public static Func<object, object[]> CreateGetPropertyValuesDelegate(Type type, PropertyInfo[] propertyInfos)
        {
            DynamicMethod dm = new DynamicMethod($"{type.Name}_GetSomePropertyValues_{Guid.NewGuid()}", typeof(object[]), new[] { typeof(object) }, true);
            ILGenerator il = dm.GetILGenerator();

            LocalBuilder rtArray = il.DeclareLocal(typeof(object[]));
            LocalBuilder typeValueLocal = il.DeclareLocal(type);

            //objectLocal = arg_0
            il.Emit(OpCodes.Ldarg_0);//[object-value]
            il.Emit(OpCodes.Unbox_Any, type); //[type-value]
            il.Emit(OpCodes.Stloc, typeValueLocal);//empty

            //rtArray = new object[]
            EmitUtil.EmitInt32(il, propertyInfos.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, rtArray);

            int index = 0;
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                Type propertyType = propertyInfo.PropertyType;
                MethodInfo getMethodInfo = propertyInfo.GetGetMethod()!;

                il.Emit(OpCodes.Ldloc, rtArray);//[rtList]

                EmitUtil.EmitGetPropertyValue(il, typeValueLocal, propertyInfo); //[rtList]["propertyName="][propertyInfo-object-value]

                //[rtList][propertyInfo-object-value]

                EmitUtil.EmitInt32(il, index);//[rtList][propertyInfo-object-value][index]

                il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArraySetValueMethod, null);

                index++;
            }

            il.Emit(OpCodes.Ldloc, rtArray);

            il.Emit(OpCodes.Ret);

            Type funType = Expression.GetFuncType(typeof(object), typeof(object[]));

            return (Func<object, object[]>)dm.CreateDelegate(funType);
        }

        /// <summary>
        /// 创建 将object转换为query字符串的 代理
        /// </summary>
        public static Func<object, List<string>> CreateConvertPropertiesToQueriesDelegate(Type type, IEnumerable<PropertyInfo> propertyInfos)
        {
            DynamicMethod dm = new DynamicMethod($"{type.Name}_GetSomePropertyValues_{Guid.NewGuid()}", typeof(List<string>), new[] { typeof(object) }, true);
            ILGenerator il = dm.GetILGenerator();

            LocalBuilder rtList = il.DeclareLocal(typeof(List<string>));
            LocalBuilder objectValueLocal = il.DeclareLocal(type);

            //objectValueLocal = arg_0
            il.Emit(OpCodes.Ldarg_0);//[object-value]
            il.Emit(OpCodes.Unbox_Any, type); //[type-value]
            il.Emit(OpCodes.Stloc, objectValueLocal);//empty

            //rtList = new List<string>()
            il.Emit(OpCodes.Newobj, CommonReflectionInfos.StringListConstructorInfo);
            il.Emit(OpCodes.Stloc, rtList);

            EmitPropertiesToQueries(il, propertyInfos, objectValueLocal, rtList, null);

            il.Emit(OpCodes.Ldloc, rtList);

            il.Emit(OpCodes.Ret);

            Type funType = Expression.GetFuncType(typeof(object), typeof(List<string>));

            return (Func<object, List<string>>)dm.CreateDelegate(funType);

        }

        private static void EmitPropertiesToQueries(ILGenerator il, IEnumerable<PropertyInfo> propertyInfos, LocalBuilder objectValueLocal, LocalBuilder rtList, string? queryNamePrefix)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                Type propertyType = propertyInfo.PropertyType;
                MethodInfo getMethodInfo = propertyInfo.GetGetMethod()!;

                LocalBuilder localBoxedPropertyValue = il.DeclareLocal(typeof(object));

                EmitUtil.EmitGetPropertyValue(il, objectValueLocal, propertyInfo); //[propertyInfo-boxed-value]

                il.Emit(OpCodes.Stloc, localBoxedPropertyValue);

                #region to string

                //1. 判断array
                //2. POCO
                //3. 复杂类
                //查询ConvertToString ，如果有，则调用，如果没有，则遍历内部属性
                //如果为poco，直接tostring， 借鉴database做法，tostring converter dict
                //如果为array,遍历每一个
                //如果为负责类型，

                if (ReflectionUtil.IsValueTypeOrString(propertyType))
                {
                    il.Emit(OpCodes.Ldloc, rtList);//[rtList]

                    #region Prepare List Item

                    il.Emit(OpCodes.Ldstr, string.IsNullOrEmpty(queryNamePrefix) ? $"{propertyInfo.Name}=" : $"{queryNamePrefix}.{propertyInfo.Name}="); //[rtList]["propertyName="]

                    il.Emit(OpCodes.Ldloc, localBoxedPropertyValue);

                    EmitUtil.EmitLoadType(il, propertyType);

                    EmitUtil.EmitInt32(il, (int)StringConvertPurpose.HTTP_QUERY);

                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ConvertToStringMethod, null); //[rtList]["propertyName="][propertyInfo-string]

                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringConcatMethod, null);//[rtArrray]["propertyName=propertyInfo-string"]

                    #endregion

                    //Add to rtList
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringListAddMethod, null);//emtpy
                }
                else if (propertyType.IsArray)
                {
                    if (!ReflectionUtil.IsValueTypeOrString(propertyType.GetElementType()))
                    {
                        throw new NotSupportedException("不支持非基础类型的数组");
                    }

                    LocalBuilder localI = il.DeclareLocal(typeof(int));
                    LocalBuilder localArrayLength = il.DeclareLocal(typeof(int));

                    Label labelCondition = il.DefineLabel();
                    Label labelTrue = il.DefineLabel();

                    //Get localArrayLength
                    il.Emit(OpCodes.Ldloc, localBoxedPropertyValue);//[propertyInfo-boxed-value]
                    EmitUtil.EmitInt32(il, 0); //[propertyInfo-boxed-value][0]
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArrayGetLengthMethod, null); //[length]
                    il.Emit(OpCodes.Stloc, localArrayLength); //empty

                    #region for loop

                    //i = 0
                    EmitUtil.EmitInt32(il, 0);
                    il.Emit(OpCodes.Stloc, localI);

                    //goto condition
                    il.Emit(OpCodes.Br, labelCondition);

                    //true loop
                    il.MarkLabel(labelTrue);

                    #region InnerOperation

                    il.Emit(OpCodes.Ldloc, rtList);//[rtList]

                    #region Prepare List Item

                    il.Emit(OpCodes.Ldstr, string.IsNullOrEmpty(queryNamePrefix) ? $"{propertyInfo.Name}=" : $"{queryNamePrefix}.{propertyInfo.Name}="); //[rtList]["propertyName="]

                    il.Emit(OpCodes.Ldloc, localBoxedPropertyValue); //[rtList]["propertyName="][Boxed-Array]

                    il.Emit(OpCodes.Ldloc, localI); //[rtList]["propertyName="][Boxed-Array][i]

                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArrayGetValueMethod, null); //[rtList]["propertyName="][item]

                    EmitUtil.EmitLoadType(il, propertyType.GetElementType()!);//[rtList]["propertyName="][item][itemType]
                    EmitUtil.EmitInt32(il, (int)StringConvertPurpose.HTTP_QUERY);
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ConvertToStringMethod, null); //[rtList]["propertyName="][item-string]
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringConcatMethod, null);//[rtArrray]["propertyName=item-string"]

                    #endregion

                    // Add To List
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringListAddMethod, null);//empty

                    #endregion

                    //i++
                    il.Emit(OpCodes.Ldloc, localI);
                    EmitUtil.EmitInt32(il, 1);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Stloc, localI);

                    //condition
                    il.MarkLabel(labelCondition);
                    il.Emit(OpCodes.Ldloc, localI);
                    il.Emit(OpCodes.Ldloc, localArrayLength);
                    il.Emit(OpCodes.Clt);
                    il.Emit(OpCodes.Brtrue, labelTrue);

                    //for end

                    #endregion

                }
                else if (typeof(IEnumerable).IsAssignableFrom(propertyType))
                {
                    if (!propertyType.IsGenericType)
                    {
                        throw new NotSupportedException("不支持非基础类型的数组");
                    }

                    LocalBuilder localEnumerator = il.DeclareLocal(typeof(IEnumerator));
                    Label labelCondition = il.DefineLabel();
                    Label labelTrue = il.DefineLabel();

                    //get enumerator
                    il.Emit(OpCodes.Ldloc, localBoxedPropertyValue);//[propertyValue]
                    il.EmitCall(OpCodes.Callvirt, CommonReflectionInfos.IEnumerableGetEnumeratorMethod, null);//[emulator]
                    il.Emit(OpCodes.Stloc, localEnumerator);

                    //goto condition
                    il.Emit(OpCodes.Br, labelCondition);

                    //True loop
                    il.MarkLabel(labelTrue);

                    #region InnerOperation

                    il.Emit(OpCodes.Ldloc, rtList);//[rtList]

                    #region Prepare List Item
                    il.Emit(OpCodes.Ldstr, string.IsNullOrEmpty(queryNamePrefix) ? $"{propertyInfo.Name}=" : $"{queryNamePrefix}.{propertyInfo.Name}="); //[rtList]["propertyName="]

                    il.Emit(OpCodes.Ldloc, localEnumerator); //[rtList]["propertyName="][enumerator]

                    il.EmitCall(OpCodes.Callvirt, CommonReflectionInfos.EnumeratorGetCurrentMethod, null); //[rtList]["propertyName="][item]

                    EmitUtil.EmitLoadType(il, propertyType.GetGenericArguments()[0]);//[rtList]["propertyName="][item][itemType]
                    EmitUtil.EmitInt32(il, (int)StringConvertPurpose.HTTP_QUERY);
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ConvertToStringMethod, null); //[rtList]["propertyName="][item-string]
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringConcatMethod, null);//[rtArrray]["propertyName=item-string"]

                    #endregion

                    //Add to List
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringListAddMethod, null);//empty

                    #endregion

                    //condition
                    il.MarkLabel(labelCondition);
                    il.Emit(OpCodes.Ldloc, localEnumerator);
                    il.EmitCall(OpCodes.Callvirt, CommonReflectionInfos.EnumeratorMoveNextMethod, null);
                    il.Emit(OpCodes.Brtrue, labelTrue);

                }
                else
                {
                    //复杂类遍历Properties

                    string newQueryNamePrefix = string.IsNullOrEmpty(queryNamePrefix) ? propertyInfo.Name : $"{queryNamePrefix}.{propertyInfo.Name}";

                    EmitPropertiesToQueries(il, propertyType.GetProperties(), localBoxedPropertyValue, rtList, newQueryNamePrefix);
                }

                #endregion
            }
        }

        #endregion

    }
}
