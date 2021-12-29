using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Engine;


namespace HB.FullStack.Database.Mapper
{
    internal static class EntityMapperDelegateCreator
    {
        /// <summary>
        /// 缓存构建key时，应该包含def，startindex，length, returnNullIfFirstNull。engineType, Reader因为返回字段顺序固定了，不用加入key中
        /// </summary>
        public static Func<IEntityDefFactory, IDataReader, object> CreateToEntityDelegate(EntityDef def, IDataReader reader, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType)
        {
            DynamicMethod dm = new DynamicMethod("ToEntity" + Guid.NewGuid().ToString(), def.EntityType, new[] {typeof(IEntityDefFactory), typeof(IDataReader) }, true);
            ILGenerator il = dm.GetILGenerator();

            EmitEntityMapper(def, reader, startIndex, length, returnNullIfFirstNull, engineType, il);

            Type funcType = Expression.GetFuncType(typeof(IEntityDefFactory), typeof(IDataReader), def.EntityType);
            return (Func<IEntityDefFactory, IDataReader, object>)dm.CreateDelegate(funcType);
        }

        private static void EmitEntityMapper(EntityDef def, IDataReader reader, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType, ILGenerator il)
        {
            try
            {
                List<EntityPropertyDef> propertyDefs = new List<EntityPropertyDef>();

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    propertyDefs.Add(def.GetPropertyDef(reader.GetName(i))
                        ?? throw DatabaseExceptions.EntityError(def.EntityFullName, reader.GetName(i), "Lack PropertyDef"));
                }

                LocalBuilder returnValueLocal = il.DeclareLocal(def.EntityType);
                LocalBuilder enumStringTempLocal = il.DeclareLocal(typeof(string));
                LocalBuilder timespanZeroLocal = il.DeclareLocal(typeof(TimeSpan));
                LocalBuilder entityTypeLocal = il.DeclareLocal(typeof(Type));

                System.Reflection.Emit.Label allFinished = il.DefineLabel();

                ConstructorInfo ctor = def.EntityType.GetDefaultConstructor()
                    ?? throw DatabaseExceptions.EntityError(def.EntityFullName, "", "实体没有默认构造函数");

                il.Emit(OpCodes.Ldtoken, def.EntityType);
                il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null);
                il.Emit(OpCodes.Stloc, entityTypeLocal);

                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Stloc, returnValueLocal);
                il.Emit(OpCodes.Ldloc, returnValueLocal);//stack is now [target]

                bool firstProperty = true;

                for (int index = 0; index < propertyDefs.Count; ++index)
                {
                    bool hasConverter = false;

                    Label dbNullLabel = il.DefineLabel();
                    Label finishLable = il.DefineLabel();

                    Type dbValueType = reader.GetFieldType(index + startIndex);

                    il.Emit(OpCodes.Dup); // stack is now [target][target]

                    EntityPropertyDef propertyDef = propertyDefs[index];
                    Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

                    //======属性自定义Converter
                    if (propertyDef.TypeConverter != null)
                    {
                        il.Emit(OpCodes.Ldarg_0);//stack is now [target][target][IEntityDefFactory]
                        il.Emit(OpCodes.Ldloc, entityTypeLocal);//stack is now [target][target][IEntityDefFactory][EntityType]
                        il.Emit(OpCodes.Ldstr, propertyDef.Name);//stack is now [target][target][IEntityDefFactory][EntityType][PropertyName]
                        il.EmitCall(OpCodes.Callvirt, _getPropertyTypeConverterMethod2, null);// stack is now [target][target][TypeConverter]

                        hasConverter = true;
                    }
                    else
                    {
                        //======全局Converter

                        ITypeConverter? globalTypeConverter = TypeConvert.GetGlobalTypeConverter(trueType, engineType);

                        if (globalTypeConverter != null)
                        {
                            il.Emit(OpCodes.Ldtoken, trueType);
                            il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null);
                            EmitInt32(il, (int)engineType);

                            il.EmitCall(OpCodes.Call, _getGlobalTypeConverterMethod, null);//stack is now [target][target][TypeConverter]

                            hasConverter = true;
                        }
                    }

                    //===获取数据库值=========================================================================

                    il.Emit(OpCodes.Ldarg_1);//stack is now [...][reader]
                    EmitInt32(il, index + startIndex);// stack is now [...][reader][index]
                    il.EmitCall(OpCodes.Callvirt, _dataReaderGetItemMethod, null);// stack is now [...][value-as-object]

                    //处理Null
                    il.Emit(OpCodes.Dup); //stack is now [...][value-as-object][value-as-object]
                    il.Emit(OpCodes.Isinst, typeof(DBNull));//stack is now [...][value-as-object][DbNull/null]
                    il.Emit(OpCodes.Brtrue_S, dbNullLabel);//stack is now [...][value-as-object]

                    #region DbValueToTypeValue,逻辑同DatabaseConverty.DbValueToTypeValue一致
                    if (propertyDef.TypeConverter != null)
                    {
                        // stack is now [target][target][TypeConverter][value-as-object]
                        il.Emit(OpCodes.Ldtoken, propertyDef.Type);
                        il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null); //stack is now [target][target][TypeConverter][value-as-object][propertyType]
                        il.EmitCall(OpCodes.Callvirt, _getTypeConverterDbValueToTypeValueMethod, null);
                        il.Emit(OpCodes.Unbox_Any, propertyDef.Type);// stack is now [target][target][TypeValue]
                    }
                    else
                    {
                        if (hasConverter)
                        {
                            //全局Converter

                            // stack is now [target][target][TypeConverter][value-as-object]
                            il.Emit(OpCodes.Ldtoken, trueType);
                            il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null);//stack is now [target][target][TypeConverter][value-as-object][trueType]
                            il.EmitCall(OpCodes.Callvirt, _getTypeConverterDbValueToTypeValueMethod, null);
                            il.Emit(OpCodes.Unbox_Any, trueType);// stack is now [target][target][TypeValue]
                        }
                        else
                        {
                            //默认
                            // stack is now [target][target][value-as-object]

                            if (trueType.IsEnum)
                            {
                                // stack is now [target][target][value-as-object]
                                il.Emit(OpCodes.Castclass, typeof(string));//stack is now [target][target][string]

                                il.Emit(OpCodes.Stloc, enumStringTempLocal);//stack is now [target][target]

                                if (propertyDef.NullableUnderlyingType != null)
                                {
                                    il.Emit(OpCodes.Ldtoken, propertyDef.NullableUnderlyingType);
                                }
                                else
                                {
                                    il.Emit(OpCodes.Ldtoken, propertyDef.Type);//stack is now[target][target][propertyType-token]
                                }

                                il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null);//stack is now[target][target][propertyType]

                                il.Emit(OpCodes.Ldloc, enumStringTempLocal);//stack is now[target][target][propertyType][string]

                                il.Emit(OpCodes.Ldc_I4_1);//stack is now[target][target][propertyType][string][true]

                                il.EmitCall(OpCodes.Call, _enumParseMethod, null);//stack is now[target][target][value]

                                if (propertyDef.NullableUnderlyingType != null)
                                {
                                    il.Emit(OpCodes.Unbox_Any, propertyDef.NullableUnderlyingType);
                                }
                                else
                                {
                                    il.Emit(OpCodes.Unbox_Any, propertyDef.Type);
                                }

                                //stack is now[target][target][typed-value]
                            }
                            else
                            {
                                // stack is now [target][target][value-as-object]

                                if (dbValueType == trueType)
                                {
                                    il.Emit(OpCodes.Unbox_Any, trueType);
                                }
                                else
                                {
                                    FlexibleConvertBoxedFromHeadOfStack(il, dbValueType, trueType);
                                }

                                //stack is now[target][target][typed-value]
                            }
                        }

                        if (propertyDef.NullableUnderlyingType != null)
                        {
                            il.Emit(OpCodes.Newobj, propertyDef.Type.GetConstructor(new Type[] { propertyDef.NullableUnderlyingType })!);
                        }
                    }

                    #endregion

                    //===赋值================================================================================
                    // stack is now [target][target][TypeValue]

                    if (propertyDef.Type.IsValueType)
                    {
                        il.EmitCall(OpCodes.Call, propertyDef.SetMethod, null);
                    }
                    else
                    {
                        il.EmitCall(OpCodes.Callvirt, propertyDef.SetMethod, null);
                    }

                    //stack is now[target]
                    il.Emit(OpCodes.Br_S, finishLable);

                    il.MarkLabel(dbNullLabel);

                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Pop);

                    if (hasConverter)
                    {
                        il.Emit(OpCodes.Pop);
                    }

                    if (firstProperty && returnNullIfFirstNull)
                    {
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Ldnull); //stack is now [null]
                        il.Emit(OpCodes.Br, allFinished);
                    }

                    il.MarkLabel(finishLable);
                    firstProperty = false;
                }

                il.MarkLabel(allFinished);

                il.Emit(OpCodes.Ret);
            }
            catch (Exception ex)
            {
                //string info = ex.GetDebugInfo();
                throw DatabaseExceptions.MapperError(innerException: ex);
            }
        }

        public static Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]> CreateToParametersDelegate(EntityDef entityDef, EngineType engineType)
        {
            DynamicMethod dm = new DynamicMethod("ToParameters" + Guid.NewGuid().ToString(), typeof(KeyValuePair<string, object>[]), new[] { typeof(IEntityDefFactory), typeof(object), typeof(int) }, true);
            ILGenerator il = dm.GetILGenerator();

            LocalBuilder array = il.DeclareLocal(typeof(KeyValuePair<string, object>[]));
            LocalBuilder tmpObj = il.DeclareLocal(typeof(object));
            LocalBuilder entityTypeLocal = il.DeclareLocal(typeof(Type));
            LocalBuilder tmpTrueTypeLocal = il.DeclareLocal(typeof(Type));
            LocalBuilder entityLocal = il.DeclareLocal(entityDef.EntityType);
            LocalBuilder numberLocal = il.DeclareLocal(typeof(object));

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, entityDef.EntityType);
            il.Emit(OpCodes.Stloc, entityLocal);

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Box, typeof(int));
            il.Emit(OpCodes.Stloc, numberLocal);

            il.Emit(OpCodes.Ldtoken, entityDef.EntityType);
            il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null);
            il.Emit(OpCodes.Stloc, entityTypeLocal);

            EmitInt32(il, entityDef.FieldCount);
            il.Emit(OpCodes.Newarr, typeof(KeyValuePair<string, object>));
            il.Emit(OpCodes.Stloc, array);

            int index = 0;
            foreach (var propertyDef in entityDef.PropertyDefs)
            {
                Label nullLabel = il.DefineLabel();
                Label finishLabel = il.DefineLabel();

                il.Emit(OpCodes.Ldloc, array);//[array]

                il.Emit(OpCodes.Ldstr, $"{propertyDef.DbParameterizedName!}_");

                il.Emit(OpCodes.Ldloc, numberLocal);

                il.EmitCall(OpCodes.Call, _getStringConcatMethod, null);//[array][key]


                il.Emit(OpCodes.Ldloc, entityLocal);//[array][key][entity]

                if (propertyDef.Type.IsValueType)
                {
                    il.EmitCall(OpCodes.Call, propertyDef.GetMethod, null);
                    il.Emit(OpCodes.Box, propertyDef.Type);
                }
                else
                {
                    il.EmitCall(OpCodes.Callvirt, propertyDef.GetMethod, null);
                }

                //[array][key][property_value_obj]

                #region TypeValue To DbValue

                //判断是否是null
                il.Emit(OpCodes.Dup);//[array][key][property_value_obj][property_value_obj]
                il.Emit(OpCodes.Brfalse_S, nullLabel);//[array][key][property_value_obj]

                if (propertyDef.TypeConverter != null)
                {
                    il.Emit(OpCodes.Stloc, tmpObj);//[array][key]

                    il.Emit(OpCodes.Ldarg_0); //[array][key][IEntityDefFactory]

                    il.Emit(OpCodes.Ldloc, entityTypeLocal);//[array][key][IEntityDefFactory][entityType]
                    //emiter.LoadLocal(entityTypeLocal);

                    il.Emit(OpCodes.Ldstr, propertyDef.Name);//[array][key][IEntityDefFactory][entityType][propertyName]
                    il.EmitCall(OpCodes.Callvirt, _getPropertyTypeConverterMethod2, null);//[array][key][typeconverter]

                    il.Emit(OpCodes.Ldloc, tmpObj);
                    //emiter.LoadLocal(tmpObj);
                    //[array][key][typeconveter][property_value_obj]
                    il.Emit(OpCodes.Ldtoken, propertyDef.Type);
                    //emiter.LoadConstant(propertyDef.Type);
                    il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null);
                    //emiter.Call(EntityMapperDelegateCreator._getTypeFromHandleMethod);
                    //[array][key][typeconveter][property_value_obj][property_type]
                    il.EmitCall(OpCodes.Callvirt, _getTypeConverterTypeValueToDbValueMethod, null);
                    //emiter.CallVirtual(typeof(ITypeConverter).GetMethod(nameof(ITypeConverter.TypeValueToDbValue)));
                    //[array][key][db_value]
                }
                else
                {
                    Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

                    //查看全局TypeConvert

                    ITypeConverter? globalConverter = TypeConvert.GetGlobalTypeConverter(trueType, engineType);

                    if (globalConverter != null)
                    {
                        il.Emit(OpCodes.Stloc, tmpObj);
                        //emiter.StoreLocal(tmpObj);
                        //[array][key]

                        il.Emit(OpCodes.Ldtoken, trueType);
                        //emiter.LoadConstant(trueType);
                        il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null);
                        //emiter.Call(EntityMapperDelegateCreator._getTypeFromHandleMethod);
                        il.Emit(OpCodes.Stloc, tmpTrueTypeLocal);
                        //emiter.StoreLocal(tmpTrueTypeLocal);
                        il.Emit(OpCodes.Ldloc, tmpTrueTypeLocal);
                        //emiter.LoadLocal(tmpTrueTypeLocal);

                        EmitInt32(il, (int)engineType);
                        //emiter.LoadConstant((int)engineType);
                        il.EmitCall(OpCodes.Call, _getGlobalTypeConverterMethod, null);
                        //emiter.Call(EntityMapperDelegateCreator._getGlobalTypeConverterMethod);
                        //[array][key][typeconverter]

                        il.Emit(OpCodes.Ldloc, tmpObj);
                        //emiter.LoadLocal(tmpObj);
                        //[array][key][typeconverter][property_value_obj]
                        il.Emit(OpCodes.Ldloc, tmpTrueTypeLocal);
                        //emiter.LoadLocal(tmpTrueTypeLocal);
                        //[array][key][typeconverter][property_value_obj][true_type]
                        il.EmitCall(OpCodes.Callvirt, _getTypeConverterTypeValueToDbValueMethod, null);
                        //emiter.CallVirtual(typeof(ITypeConverter).GetMethod(nameof(ITypeConverter.TypeValueToDbValue)));
                        //[array][key][db_value]
                    }
                    else
                    {
                        //默认
                        if (trueType.IsEnum)
                        {
                            il.EmitCall(OpCodes.Callvirt, _getObjectToStringMethod, null);
                            //emiter.CallVirtual(_getObjectToStringMethod);
                        }
                    }
                }

                il.Emit(OpCodes.Br_S, finishLabel);
                ////emiter.Branch(finishLabel);

                #endregion



                #region If Null 
                il.MarkLabel(nullLabel);
                //emiter.MarkLabel(nullLabel);
                //[array][key][property_value_obj]

                il.Emit(OpCodes.Pop);
                //emiter.Pop();
                //[array][key]


                il.Emit(OpCodes.Ldsfld, _dbNullValueFiled);
                //emiter.LoadField(typeof(DBNull).GetField("Value"));
                //[array][key][DBNull]

                //il.Emit(OpCodes.Br_S, finishLabel);
                ////emiter.Branch(finishLabel);
                #endregion


                il.MarkLabel(finishLabel);
                ////emiter.MarkLabel(finishLabel);



                var kvCtor = typeof(KeyValuePair<string, object>).GetConstructor(new Type[] { typeof(string), typeof(object) })!;

                il.Emit(OpCodes.Newobj, kvCtor);
                //emiter.NewObject(kvCtor);
                //[array][kv]



                il.Emit(OpCodes.Box, typeof(KeyValuePair<string, object>));
                //emiter.Box<KeyValuePair<string, object>>();
                //[array][kv_obj]

                EmitInt32(il, index);
                //emiter.LoadConstant(index);
                //[array][kv_obj][index]

                il.EmitCall(OpCodes.Call, _getArraySetValueMethod, null);
                //emiter.Call(typeof(Array).GetMethod(nameof(Array.SetValue), new Type[] { typeof(object), typeof(int) }));

                index++;
            }

            il.Emit(OpCodes.Ldloc, array);
            //emiter.LoadLocal(array);

            il.Emit(OpCodes.Ret);
            //emiter.Return();


            Type funType = Expression.GetFuncType(typeof(IEntityDefFactory), typeof(object), typeof(int), typeof(KeyValuePair<string, object>[]));

            return (Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]>)dm.CreateDelegate(funType);

            //return emiter.CreateDelegate();
        }

        private static MethodInfo? ResolveOperator(MethodInfo[] methods, Type from, Type to, string name)
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

        private static MethodInfo? GetOperator(Type from, Type to)
        {
            if (to == null) return null;
            MethodInfo[] fromMethods, toMethods;
            return ResolveOperator(fromMethods = from.GetMethods(BindingFlags.Static | BindingFlags.Public), from, to, "op_Implicit")
                ?? ResolveOperator(toMethods = to.GetMethods(BindingFlags.Static | BindingFlags.Public), from, to, "op_Implicit")
                ?? ResolveOperator(fromMethods, from, to, "op_Explicit")
                ?? ResolveOperator(toMethods, from, to, "op_Explicit");
        }

        private static void FlexibleConvertBoxedFromHeadOfStack(ILGenerator il, Type from, Type to)
        {
            MethodInfo? op;
            if (from == to)
            {
                il.Emit(OpCodes.Unbox_Any, to); // stack is now [target][target][typed-value]
            }
            else if ((op = GetOperator(from, to)) != null)
            {
                // this is handy for things like decimal <===> double
                il.Emit(OpCodes.Unbox_Any, from); // stack is now [target][target][data-typed-value]
                il.Emit(OpCodes.Call, op); // stack is now [target][target][typed-value]
            }
            else
            {
                bool handled = false;
                OpCode opCode = default;
                switch (Type.GetTypeCode(from))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        handled = true;
                        switch (Type.GetTypeCode(to))
                        {
                            case TypeCode.Byte:
                                opCode = OpCodes.Conv_Ovf_I1_Un; break;
                            case TypeCode.SByte:
                                opCode = OpCodes.Conv_Ovf_I1; break;
                            case TypeCode.UInt16:
                                opCode = OpCodes.Conv_Ovf_I2_Un; break;
                            case TypeCode.Int16:
                                opCode = OpCodes.Conv_Ovf_I2; break;
                            case TypeCode.UInt32:
                                opCode = OpCodes.Conv_Ovf_I4_Un; break;
                            case TypeCode.Boolean: // boolean is basically an int, at least at this level
                            case TypeCode.Int32:
                                opCode = OpCodes.Conv_Ovf_I4; break;
                            case TypeCode.UInt64:
                                opCode = OpCodes.Conv_Ovf_I8_Un; break;
                            case TypeCode.Int64:
                                opCode = OpCodes.Conv_Ovf_I8; break;
                            case TypeCode.Single:
                                opCode = OpCodes.Conv_R4; break;
                            case TypeCode.Double:
                                opCode = OpCodes.Conv_R8; break;
                            default:
                                handled = false;
                                break;
                        }
                        break;
                }
                if (handled)
                {
                    il.Emit(OpCodes.Unbox_Any, from); // stack is now [target][target][col-typed-value]
                    il.Emit(opCode); // stack is now [target][target][typed-value]
                    if (to == typeof(bool))
                    { // compare to zero; I checked "csc" - this is the trick it uses; nice
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldtoken, to); // stack is now [target][target][value][member-type-token]
                    il.EmitCall(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!, null); // stack is now [target][target][value][member-type]
                    il.EmitCall(OpCodes.Call, _invariantCultureMethod, null); // stack is now [target][target][value][member-type][culture]
                    il.EmitCall(OpCodes.Call, typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[] { typeof(object), typeof(Type), typeof(IFormatProvider) })!, null); // stack is now [target][target][boxed-member-type-value]
                    il.Emit(OpCodes.Unbox_Any, to); // stack is now [target][target][typed-value]
                }
            }
        }

        private static void EmitInt32(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }

        private static readonly MethodInfo _dataReaderGetItemMethod = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)!
            .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))!
            .Select(p => p.GetGetMethod()).First()!;

        private static readonly MethodInfo _enumParseMethod = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) })!;

        private static readonly MethodInfo _invariantCultureMethod = typeof(CultureInfo).GetProperty(nameof(CultureInfo.InvariantCulture), BindingFlags.Public | BindingFlags.Static)!.GetGetMethod()!;

        private static readonly MethodInfo _getPropertyTypeConverterMethod2 = typeof(IEntityDefFactory).GetMethod(nameof(IEntityDefFactory.GetPropertyTypeConverter))!;

        private static readonly MethodInfo _getGlobalTypeConverterMethod = typeof(TypeConvert).GetMethod(nameof(TypeConvert.GetGlobalTypeConverter), new Type[] { typeof(Type), typeof(int) })!;

        private static readonly MethodInfo _getTypeFromHandleMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!;

        private static readonly MethodInfo _getTypeConverterDbValueToTypeValueMethod = typeof(ITypeConverter).GetMethod(nameof(ITypeConverter.DbValueToTypeValue))!;

        private static readonly MethodInfo _getTypeConverterTypeValueToDbValueMethod = typeof(ITypeConverter).GetMethod(nameof(ITypeConverter.TypeValueToDbValue))!;

        private static readonly MethodInfo _getStringConcatMethod = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object), typeof(object) })!;

        private static readonly MethodInfo _getObjectToStringMethod = typeof(object).GetMethod(nameof(object.ToString))!;

        private static readonly FieldInfo _dbNullValueFiled = typeof(DBNull).GetField("Value")!;

        private static readonly MethodInfo _getArraySetValueMethod = typeof(Array).GetMethod(nameof(Array.SetValue), new Type[] { typeof(object), typeof(int) })!;
    }
}