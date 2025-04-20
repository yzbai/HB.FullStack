using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace System
{
    public static class PropertyDelegateCreator
    {
        public static Func<object, object?> CreateGetDelegate(PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(property);

            MethodInfo? getMethod = property.GetGetMethod(true)
                ?? throw new ArgumentException($"In {nameof(PropertyDelegateCreator)}.{nameof(CreateGetDelegate)}, {nameof(property.DeclaringType.FullName)}.{nameof(property.Name)} do not have a GET Method.");

            Type declaringType = property.DeclaringType
                ?? throw new ArgumentException($"In {nameof(PropertyDelegateCreator)}.{nameof(CreateGetDelegate)}, {nameof(property.Name)} do not have a DeclareTyping.");

            DynamicMethod dm = new DynamicMethod(
                $"GetDelegate_{declaringType.Name}_{property.Name}_{Guid.CreateVersion7()}",
                typeof(object),
                [typeof(object)],
                declaringType,
                true);

            ILGenerator il = dm.GetILGenerator();
            LocalBuilder inputObjectLocal = il.DeclareLocal(typeof(object));

            if (!getMethod.IsStatic)
            {
                il.ThrowIfArgumentIsNull(
                    0,
                    "inputObject",
                    $"At {dm.Name}, Input Object is Null.");

                //赋值给inputObjectLocal
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stloc, inputObjectLocal);
            }

            il.EmitGetPropertyValue(inputObjectLocal, property);

            il.Emit(OpCodes.Ret);

            return (Func<object, object?>)dm.CreateDelegate(typeof(Func<object, object?>));
        }

        public static Action<object, object?> CreateSetDelegate(PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(property);

            MethodInfo setMethod = property.GetSetMethod(true)
                ?? throw new ArgumentException($"In {nameof(PropertyDelegateCreator)}.{nameof(CreateSetDelegate)}, {nameof(property.DeclaringType.FullName)}.{nameof(property.Name)} do not have a SET Method.");

            Type declaringType = property.DeclaringType
                ?? throw new ArgumentException($"In {nameof(PropertyDelegateCreator)}.{nameof(CreateSetDelegate)}, {nameof(property.Name)} do not have a DeclareTyping.");

            DynamicMethod dm = new DynamicMethod(
                $"SetDelegate_{declaringType.Name}_{property.Name}_{Guid.CreateVersion7()}",
                null,
                [typeof(object), typeof(object)],
                declaringType,
                true);

            ILGenerator il = dm.GetILGenerator();

            LocalBuilder objectLocal = il.DeclareLocal(typeof(object));
            LocalBuilder propertyValueLocal = il.DeclareLocal(typeof(object));

            if (!setMethod.IsStatic)
            {
                il.ThrowIfArgumentIsNull(
                    0,
                    "inputObject",
                    $"At {dm.Name}  Input Object is Null.");

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stloc, objectLocal);
            }

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc, propertyValueLocal);

            il.EmitSetPropertyValue(objectLocal, propertyValueLocal, property);

            il.Emit(OpCodes.Ret);

            return (Action<object, object?>)dm.CreateDelegate(typeof(Action<object, object?>));
        }

        public static Func<object, object?[]> CreateBatchGetDelegate(IList<PropertyInfo> propertyInfos, Type type)
        {
            ThrowIf.NullOrEmpty(propertyInfos, nameof(propertyInfos));

            DynamicMethod dm = new DynamicMethod(
                $"BatchGetDelegate_{type.Name}_{Guid.CreateVersion7()}",
                typeof(object?[]),
                [typeof(object)],
                true);

            EmitBatchGetDelegateCode(false, dm, propertyInfos, type);

            Type funType = Expression.GetFuncType(typeof(object), typeof(object?[]));

            return (Func<object, object?[]>)dm.CreateDelegate(funType);
        }

        public static Func<object, NameValuePair[]> CreateBatchGetDelegate2(IList<PropertyInfo> propertyInfos, Type type)
        {
            ThrowIf.NullOrEmpty(propertyInfos, nameof(propertyInfos));

            DynamicMethod dm = new DynamicMethod(
                $"BatchGetDelegate2_{type.Name}_{Guid.CreateVersion7()}",
                typeof(NameValuePair[]),
                [typeof(object)],
                true);

            EmitBatchGetDelegateCode(true, dm, propertyInfos, type);

            Type funType = Expression.GetFuncType(typeof(object), typeof(NameValuePair[]));

            return (Func<object, NameValuePair[]>)dm.CreateDelegate(funType);
        }

        private static void EmitBatchGetDelegateCode(bool returntNameValuePair, DynamicMethod dm, IList<PropertyInfo> propertyInfos, Type type)
        {
            ILGenerator il = dm.GetILGenerator();

            LocalBuilder rtArray = il.DeclareLocal(typeof(object?[]));
            LocalBuilder inputObjectValueLocal = il.DeclareLocal(type);

            //objectLocal = arg_0
            il.Emit(OpCodes.Ldarg_0);//[object-value]
            il.Emit(OpCodes.Unbox_Any, type); //[type-value]
            il.Emit(OpCodes.Stloc, inputObjectValueLocal);//empty

            //rtArray = new object[]
            il.EmitInt32(propertyInfos.Count);
            il.Emit(OpCodes.Newarr, returntNameValuePair ? typeof(NameValuePair) : typeof(object));
            il.Emit(OpCodes.Stloc, rtArray);

            bool needCheckInputObject = true;
            int index = 0;

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                //如果碰到不是Static的PropertyInfo，那么需要检查inputObject是否是null
                if (needCheckInputObject && !propertyInfo.GetGetMethod()!.IsStatic)
                {
                    il.ThrowIfArgumentIsNull(
                        0,
                        "inputObject",
                        $"At {dm.Name} Input Object is Null.");

                    needCheckInputObject = false;
                }

                il.Emit(OpCodes.Ldloc, rtArray);//[rtArray]

                if (returntNameValuePair)
                {
                    il.Emit(OpCodes.Ldstr, propertyInfo.Name);//[rtArray][propertyName]
                }

                il.EmitGetPropertyValue(inputObjectValueLocal, propertyInfo); //[rtArray][propertyName][propertyInfo-object-value]

                if (returntNameValuePair)
                {
                    il.Emit(OpCodes.Newobj, CommonReflectionInfos.PropertyValueConstructorInfo); //[rtArray][propertyValue]
                }

                il.EmitInt32(index);//[rtList][propertyValue][index]

                il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArraySetValueMethod, null);

                index++;
            }

            il.Emit(OpCodes.Ldloc, rtArray);

            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// 创建 将Property转换为{PropertyName}={PropertyValueString}这样query字符串的 代理
        /// </summary>
        public static Func<object, List<string>> CreateGetQueryStringDelegate(IList<PropertyInfo> propertyInfos, Type type)
        {
            DynamicMethod dm = new DynamicMethod(
                $"{type.FullName}_{nameof(CreateGetQueryStringDelegate)}_{Guid.NewGuid()}",
                typeof(List<string>),
                [typeof(object)],
                true);

            ILGenerator il = dm.GetILGenerator();

            LocalBuilder rtList = il.DeclareLocal(typeof(List<string>));
            LocalBuilder objectValueLocal = il.DeclareLocal(type);

            il.ThrowIfArgumentIsNull(0, "inputObject", $"At {dm.Name}, InputObject is Null");

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

        private static void EmitPropertiesToQueries(
            ILGenerator il,
            IList<PropertyInfo> propertyInfos,
            LocalBuilder inputObject,
            LocalBuilder rtList,
            string? queryNamePrefix)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                Type propertyType = propertyInfo.PropertyType;

                LocalBuilder curBoxedPropertyValue = il.DeclareLocal(typeof(object));

                il.EmitGetPropertyValue(inputObject, propertyInfo); //[propertyInfo-boxed-value]

                il.Emit(OpCodes.Stloc, curBoxedPropertyValue);

                #region to string

                //1. 判断array
                //2. POCO
                //3. 复杂类
                //查询ConvertToString ，如果有，则调用，如果没有，则遍历内部属性
                //如果为poco，直接tostring， 借鉴database做法，tostring converter dict
                //如果为array,遍历每一个
                //如果为负责类型，

                if (propertyType.IsValueTypeOrString())
                {
                    il.Emit(OpCodes.Ldloc, rtList);//[rtList]

                    #region generate query string

                    il.Emit(OpCodes.Ldstr, string.IsNullOrEmpty(queryNamePrefix) ? $"{propertyInfo.Name}=" : $"{queryNamePrefix}.{propertyInfo.Name}="); //[rtList]["propertyName="]

                    il.Emit(OpCodes.Ldloc, curBoxedPropertyValue);

                    il.EmitLoadType(propertyType);

                    il.EmitInt32((int)StringConvertPurpose.HTTP_QUERY);

                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ConvertToStringMethod, null); //[rtList]["propertyName="][propertyInfo-string]

                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringConcatMethod, null);//[rtArrray]["propertyName=propertyInfo-string"]

                    #endregion

                    //Add to rtList
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringListAddMethod, null);//emtpy
                }
                else if (propertyType.IsArray)
                {
                    if (!propertyType.GetElementType().IsValueTypeOrString())
                    {
                        throw new NotSupportedException("不支持非基础类型的数组");
                    }

                    LocalBuilder localI = il.DeclareLocal(typeof(int));
                    LocalBuilder localArrayLength = il.DeclareLocal(typeof(int));

                    Label labelCondition = il.DefineLabel();
                    Label labelTrue = il.DefineLabel();

                    //GetDef localArrayLength
                    il.Emit(OpCodes.Ldloc, curBoxedPropertyValue);//[propertyInfo-boxed-value]
                    ILGeneratorExtensions.EmitInt32(il, 0); //[propertyInfo-boxed-value][0]
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArrayGetLengthMethod, null); //[length]
                    il.Emit(OpCodes.Stloc, localArrayLength); //empty

                    #region for loop

                    //i = 0
                    ILGeneratorExtensions.EmitInt32(il, 0);
                    il.Emit(OpCodes.Stloc, localI);

                    //goto condition
                    il.Emit(OpCodes.Br, labelCondition);

                    //true loop
                    il.MarkLabel(labelTrue);

                    #region InnerOperation

                    il.Emit(OpCodes.Ldloc, rtList);//[rtList]

                    #region Prepare List Item

                    il.Emit(OpCodes.Ldstr, string.IsNullOrEmpty(queryNamePrefix) ? $"{propertyInfo.Name}=" : $"{queryNamePrefix}.{propertyInfo.Name}="); //[rtList]["propertyName="]

                    il.Emit(OpCodes.Ldloc, curBoxedPropertyValue); //[rtList]["propertyName="][Boxed-Array]

                    il.Emit(OpCodes.Ldloc, localI); //[rtList]["propertyName="][Boxed-Array][i]

                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArrayGetValueMethod, null); //[rtList]["propertyName="][item]

                    ILGeneratorExtensions.EmitLoadType(il, propertyType.GetElementType()!);//[rtList]["propertyName="][item][itemType]
                    ILGeneratorExtensions.EmitInt32(il, (int)StringConvertPurpose.HTTP_QUERY);
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.ConvertToStringMethod, null); //[rtList]["propertyName="][item-string]
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringConcatMethod, null);//[rtArrray]["propertyName=item-string"]

                    #endregion

                    // Add To List
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringListAddMethod, null);//empty

                    #endregion

                    //i++
                    il.Emit(OpCodes.Ldloc, localI);
                    ILGeneratorExtensions.EmitInt32(il, 1);
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
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType))
                {
                    if (!propertyType.IsGenericType)
                    {
                        throw new NotSupportedException("不支持非基础类型的数组");
                    }

                    LocalBuilder localEnumerator = il.DeclareLocal(typeof(System.Collections.IEnumerable));
                    Label labelCondition = il.DefineLabel();
                    Label labelTrue = il.DefineLabel();

                    //get enumerator
                    il.Emit(OpCodes.Ldloc, curBoxedPropertyValue);//[propertyValue]
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

                    ILGeneratorExtensions.EmitLoadType(il, propertyType.GetGenericArguments()[0]);//[rtList]["propertyName="][item][itemType]
                    ILGeneratorExtensions.EmitInt32(il, (int)StringConvertPurpose.HTTP_QUERY);
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

                    EmitPropertiesToQueries(il, propertyType.GetProperties(), curBoxedPropertyValue, rtList, newQueryNamePrefix);
                }

                #endregion
            }
        }
    }
}
