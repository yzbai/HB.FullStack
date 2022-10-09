/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Convert
{
    public static partial class DbModelConvert
    {
        /// <summary>
        /// 得到一个将 数据库行 转为Model 的 delegate
        /// 缓存构建key时，应该包含def，startindex，length, returnNullIfFirstNull。engineType, Reader因为返回字段顺序固定了，不用加入key中
        /// </summary>
        public static Func<IDbModelDefFactory, IDataReader, object> CreateDataReaderRowToModelDelegate(
            DbModelDef def,
            IDataReader reader,
            int startIndex,
            int length,
            bool returnNullIfFirstNull,
            EngineType engineType)
        {
            DynamicMethod dm = new DynamicMethod("ToModel" + Guid.NewGuid().ToString(), def.ModelType, new[] { typeof(IDbModelDefFactory), typeof(IDataReader) }, true);
            ILGenerator il = dm.GetILGenerator();

            EmitDataReaderRowToModel(def, reader, startIndex, length, returnNullIfFirstNull, engineType, il);

            Type funcType = Expression.GetFuncType(typeof(IDbModelDefFactory), typeof(IDataReader), def.ModelType);
            return (Func<IDbModelDefFactory, IDataReader, object>)dm.CreateDelegate(funcType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="def"></param>
        /// <param name="reader"></param>
        /// <param name="startIndex">这一数据行，从那一列开始算</param>
        /// <param name="length">列数，即字段数</param>
        /// <param name="returnNullIfFirstNull"></param>
        /// <param name="engineType"></param>
        /// <param name="il"></param>
        private static void EmitDataReaderRowToModel(DbModelDef def, IDataReader reader, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType, ILGenerator il)
        {
            try
            {
                List<DbModelPropertyDef> propertyDefs = new List<DbModelPropertyDef>();

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    propertyDefs.Add(def.GetPropertyDef(reader.GetName(i))
                        ?? throw DatabaseExceptions.ModelError(def.ModelFullName, reader.GetName(i), "Lack PropertyDef"));
                }

                LocalBuilder returnValueLocal = il.DeclareLocal(def.ModelType);
                LocalBuilder enumStringTempLocal = il.DeclareLocal(typeof(string));
                LocalBuilder timespanZeroLocal = il.DeclareLocal(typeof(TimeSpan));
                LocalBuilder modelTypeLocal = il.DeclareLocal(typeof(Type));

                System.Reflection.Emit.Label allFinished = il.DefineLabel();

                ConstructorInfo ctor = def.ModelType.GetDefaultConstructor()
                    ?? throw DatabaseExceptions.ModelError(def.ModelFullName, "", "实体没有默认构造函数");

                il.Emit(OpCodes.Ldtoken, def.ModelType);
                il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);
                il.Emit(OpCodes.Stloc, modelTypeLocal);

                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Stloc, returnValueLocal);
                il.Emit(OpCodes.Ldloc, returnValueLocal);//stack is now [target]

                bool firstProperty = true;

                for (int index = 0; index < propertyDefs.Count; ++index)
                {
                    bool hasGlobalConverter = false;

                    Label dbNullLabel = il.DefineLabel();
                    Label finishLable = il.DefineLabel();

                    Type dbValueType = reader.GetFieldType(index + startIndex);

                    il.Emit(OpCodes.Dup); // stack is now [target][target]

                    DbModelPropertyDef propertyDef = propertyDefs[index];
                    Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

                    //======属性自定义Converter
                    if (propertyDef.TypeConverter != null)
                    {
                        il.Emit(OpCodes.Ldarg_0);//stack is now [target][target][IDbModelDefFactory]
                        il.Emit(OpCodes.Ldloc, modelTypeLocal);//stack is now [target][target][IDbModelDefFactory][ModelType]
                        il.Emit(OpCodes.Ldstr, propertyDef.Name);//stack is now [target][target][IDbModelDefFactory][ModelType][PropertyName]

                        //TODO: 不能直接把propertyDef.TypeConverter变量加进来吗?
                        il.EmitCall(OpCodes.Callvirt, _getPropertyTypeConverterMethod2, null);// stack is now [target][target][DbPropertyConverter]

                        hasGlobalConverter = true;
                    }
                    else
                    {
                        //======全局Converter

                        IDbPropertyConverter? globalTypeConverter = DbPropertyConvert.GetGlobalDbPropertyConverter(trueType, engineType);

                        if (globalTypeConverter != null)
                        {
                            il.Emit(OpCodes.Ldtoken, trueType);
                            il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);
                            EmitUtil.EmitInt32(il, (int)engineType);

                            //TODO: 不能直接把globalTypeConverter变量加进来吗?
                            il.EmitCall(OpCodes.Call, _getGlobalTypeConverterMethod, null);//stack is now [target][target][DbPropertyConverter]

                            hasGlobalConverter = true;
                        }
                    }

                    //===获取数据库值=========================================================================

                    il.Emit(OpCodes.Ldarg_1);//stack is now [...][reader]
                    EmitUtil.EmitInt32(il, index + startIndex);// stack is now [...][reader][index]
                    il.EmitCall(OpCodes.Callvirt, _dataReaderGetItemMethod, null);// stack is now [...][value-as-object]

                    //处理Null
                    il.Emit(OpCodes.Dup); //stack is now [...][value-as-object][value-as-object]
                    il.Emit(OpCodes.Isinst, typeof(DBNull));//stack is now [...][value-as-object][DbNull/null]
                    il.Emit(OpCodes.Brtrue_S, dbNullLabel);//stack is now [...][value-as-object]

                    #region DbValueToTypeValue,逻辑同DatabaseConverty.DbValueToTypeValue一致

                    if (propertyDef.TypeConverter != null) //专用Converter
                    {
                        // stack is now [target][target][DbPropertyConverter][value-as-object]
                        il.Emit(OpCodes.Ldtoken, propertyDef.Type);
                        il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null); //stack is now [target][target][DbPropertyConverter][value-as-object][propertyType]
                        il.EmitCall(OpCodes.Callvirt, _getTypeConverterDbValueToTypeValueMethod, null);
                        il.Emit(OpCodes.Unbox_Any, propertyDef.Type);// stack is now [target][target][TypeValue]
                    }
                    else
                    {
                        if (hasGlobalConverter)
                        {
                            //全局Converter

                            // stack is now [target][target][DbPropertyConverter][value-as-object]
                            il.Emit(OpCodes.Ldtoken, trueType);
                            il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);//stack is now [target][target][DbPropertyConverter][value-as-object][trueType]
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

                                il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);//stack is now[target][target][propertyType]

                                il.Emit(OpCodes.Ldloc, enumStringTempLocal);//stack is now[target][target][propertyType][string]

                                il.Emit(OpCodes.Ldc_I4_1);//stack is now[target][target][propertyType][string][true]

                                il.EmitCall(OpCodes.Call, CommonReflectionInfos.EnumParseMethod, null);//stack is now[target][target][value]

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
                                //大部分基础类型，在进出数据库时，只是装箱拆箱
                                // stack is now [target][target][value-as-object]

                                if (dbValueType == trueType)
                                {
                                    il.Emit(OpCodes.Unbox_Any, trueType);
                                }
                                else
                                {
                                    EmitUtil.FlexibleConvertBoxedFromHeadOfStack(il, dbValueType, trueType);
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

                    if (hasGlobalConverter)
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

        /// <summary>
        /// 得到一个将 (IDbModelDefFactory,Model,parameter_num_suffix)转换为键值对的delegate
        /// </summary>
        /// <param name="modelDef"></param>
        /// <param name="engineType"></param>
        /// <returns></returns>
        public static Func<IDbModelDefFactory, object, int, KeyValuePair<string, object>[]> CreateModelToDbParametersDelegate(DbModelDef modelDef, EngineType engineType)
        {
            DynamicMethod dm = new DynamicMethod("ModelToParameters" + Guid.NewGuid().ToString(), typeof(KeyValuePair<string, object>[]), new[] { typeof(IDbModelDefFactory), typeof(object), typeof(int) }, true);
            ILGenerator il = dm.GetILGenerator();

            LocalBuilder array = il.DeclareLocal(typeof(KeyValuePair<string, object>[]));
            LocalBuilder tmpObj = il.DeclareLocal(typeof(object));
            LocalBuilder modelTypeLocal = il.DeclareLocal(typeof(Type));
            LocalBuilder tmpTrueTypeLocal = il.DeclareLocal(typeof(Type));
            LocalBuilder modelLocal = il.DeclareLocal(modelDef.ModelType);
            LocalBuilder numberLocal = il.DeclareLocal(typeof(object));

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, modelDef.ModelType);
            il.Emit(OpCodes.Stloc, modelLocal);

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Box, typeof(int));
            il.Emit(OpCodes.Stloc, numberLocal);

            il.Emit(OpCodes.Ldtoken, modelDef.ModelType);
            il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);
            il.Emit(OpCodes.Stloc, modelTypeLocal);

            EmitUtil.EmitInt32(il, modelDef.FieldCount);
            il.Emit(OpCodes.Newarr, typeof(KeyValuePair<string, object>));
            il.Emit(OpCodes.Stloc, array);

            int index = 0;
            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                Label nullLabel = il.DefineLabel();
                Label finishLabel = il.DefineLabel();

                il.Emit(OpCodes.Ldloc, array);//[rtArray]

                il.Emit(OpCodes.Ldstr, $"{propertyDef.DbParameterizedName!}_");

                il.Emit(OpCodes.Ldloc, numberLocal);

                il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringConcatMethod, null);//[rtArray][key]

                il.Emit(OpCodes.Ldloc, modelLocal);//[rtArray][key][model]

                if (propertyDef.Type.IsValueType)
                {
                    il.EmitCall(OpCodes.Call, propertyDef.GetMethod, null);
                    il.Emit(OpCodes.Box, propertyDef.Type);
                }
                else
                {
                    il.EmitCall(OpCodes.Callvirt, propertyDef.GetMethod, null);
                }

                //[rtArray][key][property_value_obj]

                #region TypeValue To DbValue

                //判断是否是null
                il.Emit(OpCodes.Dup);//[rtArray][key][property_value_obj][property_value_obj]
                il.Emit(OpCodes.Brfalse_S, nullLabel);//[rtArray][key][property_value_obj]

                if (propertyDef.TypeConverter != null)
                {
                    il.Emit(OpCodes.Stloc, tmpObj);//[rtArray][key]

                    il.Emit(OpCodes.Ldarg_0); //[rtArray][key][IDbModelDefFactory]

                    il.Emit(OpCodes.Ldloc, modelTypeLocal);//[rtArray][key][IDbModelDefFactory][modelType]
                    //emiter.LoadLocal(modelTypeLocal);

                    il.Emit(OpCodes.Ldstr, propertyDef.Name);//[rtArray][key][IDbModelDefFactory][modelType][propertyName]
                    il.EmitCall(OpCodes.Callvirt, _getPropertyTypeConverterMethod2, null);//[rtArray][key][typeconverter]

                    il.Emit(OpCodes.Ldloc, tmpObj); //[rtArray][key][typeconveter][property_value_obj]
                    il.Emit(OpCodes.Ldtoken, propertyDef.Type);
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null); //[rtArray][key][typeconveter][property_value_obj][property_type]
                    il.EmitCall(OpCodes.Callvirt, _getTypeConverterTypeValueToDbValueMethod, null); //[rtArray][key][db_value]
                }
                else
                {
                    Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

                    //查看全局TypeConvert

                    IDbPropertyConverter? globalConverter = DbPropertyConvert.GetGlobalDbPropertyConverter(trueType, engineType);

                    if (globalConverter != null)
                    {
                        il.Emit(OpCodes.Stloc, tmpObj); //[rtArray][key]

                        il.Emit(OpCodes.Ldtoken, trueType);
                        il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);
                        il.Emit(OpCodes.Stloc, tmpTrueTypeLocal);
                        il.Emit(OpCodes.Ldloc, tmpTrueTypeLocal);

                        EmitUtil.EmitInt32(il, (int)engineType);
                        il.EmitCall(OpCodes.Call, _getGlobalTypeConverterMethod, null);//[rtArray][key][typeconverter]

                        il.Emit(OpCodes.Ldloc, tmpObj);//[rtArray][key][typeconverter][property_value_obj]
                        il.Emit(OpCodes.Ldloc, tmpTrueTypeLocal); //[rtArray][key][typeconverter][property_value_obj][true_type]
                        il.EmitCall(OpCodes.Callvirt, _getTypeConverterTypeValueToDbValueMethod, null);//[rtArray][key][db_value]
                    }
                    else
                    {
                        //默认
                        if (trueType.IsEnum)
                        {
                            il.EmitCall(OpCodes.Callvirt, CommonReflectionInfos.ObjectToStringMethod, null);
                        }
                    }
                }

                il.Emit(OpCodes.Br_S, finishLabel);

                #endregion

                #region If Null

                il.MarkLabel(nullLabel);
                //emiter.MarkLabel(nullLabel);
                //[rtArray][key][property_value_obj]

                il.Emit(OpCodes.Pop);
                //emiter.Pop();
                //[rtArray][key]

                il.Emit(OpCodes.Ldsfld, CommonReflectionInfos.DbNullValueFiled);
                //emiter.LoadField(typeof(DBNull).GetField("Value"));
                //[rtArray][key][DBNull]

                //il.Emit(OpCodes.Br_S, finishLabel);
                ////emiter.Branch(finishLabel);

                #endregion

                il.MarkLabel(finishLabel);
                ////emiter.MarkLabel(finishLabel);

                var kvCtor = typeof(KeyValuePair<string, object>).GetConstructor(new Type[] { typeof(string), typeof(object) })!;

                il.Emit(OpCodes.Newobj, kvCtor);
                //emiter.NewObject(kvCtor);
                //[rtArray][kv]

                il.Emit(OpCodes.Box, typeof(KeyValuePair<string, object>));
                //emiter.Box<KeyValuePair<string, object>>();
                //[rtArray][kv_obj]

                EmitUtil.EmitInt32(il, index);
                //emiter.LoadConstant(index);
                //[rtArray][kv_obj][index]

                il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArraySetValueMethod, null);
                //emiter.Call(typeof(Array).GetMethod(nameof(Array.SetValue), new Type[] { typeof(object), typeof(int) }));

                index++;
            }

            il.Emit(OpCodes.Ldloc, array);
            //emiter.LoadLocal(rtArray);

            il.Emit(OpCodes.Ret);
            //emiter.Return();

            Type funType = Expression.GetFuncType(typeof(IDbModelDefFactory), typeof(object), typeof(int), typeof(KeyValuePair<string, object>[]));

            return (Func<IDbModelDefFactory, object, int, KeyValuePair<string, object>[]>)dm.CreateDelegate(funType);

            //return emiter.CreateDelegate();
        }

        /// <summary>
        /// 固定了PropertyNames的顺序，做cache时，要做顺序
        /// </summary>
        /// <param name="modelDef"></param>
        /// <param name="engineType"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static Func<IDbModelDefFactory, object?[], string, KeyValuePair<string, object>[]> CreatePropertyValuesToParametersDelegate(DbModelDef modelDef, EngineType engineType, IList<string> propertyNames)
        {
            DynamicMethod dm = new DynamicMethod(
                "PropertyValuesToParameters" + Guid.NewGuid().ToString(),
                typeof(KeyValuePair<string, object>[]),
                new[]
                {
                    typeof(IDbModelDefFactory),
                    typeof(object?[]), //propertyValues
                    typeof(string) //parameterNameSuffix
                },
                true);

            ILGenerator il = dm.GetILGenerator();

            LocalBuilder rtArray = il.DeclareLocal(typeof(KeyValuePair<string, object>[]));
            LocalBuilder tmpObj = il.DeclareLocal(typeof(object));
            LocalBuilder modelTypeLocal = il.DeclareLocal(typeof(Type));
            LocalBuilder tmpTrueTypeLocal = il.DeclareLocal(typeof(Type));
            //LocalBuilder modelLocal = il.DeclareLocal(modelDef.ModelType);
            LocalBuilder propertyValues = il.DeclareLocal(typeof(object?[]));
            LocalBuilder parameterSuffixLocal = il.DeclareLocal(typeof(string));

            //il.Emit(OpCodes.Ldarg_1);
            //il.Emit(OpCodes.Unbox_Any, modelDef.ModelType);
            //il.Emit(OpCodes.Stloc, modelLocal);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc, propertyValues);

            il.Emit(OpCodes.Ldarg_2);
            //il.Emit(OpCodes.Box, typeof(string));
            il.Emit(OpCodes.Stloc, parameterSuffixLocal);

            il.Emit(OpCodes.Ldtoken, modelDef.ModelType);
            il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);
            il.Emit(OpCodes.Stloc, modelTypeLocal);

            EmitUtil.EmitInt32(il, propertyNames.Count);
            il.Emit(OpCodes.Newarr, typeof(KeyValuePair<string, object>));
            il.Emit(OpCodes.Stloc, rtArray);

            int index = 0;
            foreach (string propertyName in propertyNames)
            {
                DbModelPropertyDef? propertyDef = modelDef.GetPropertyDef(propertyName);

                if (propertyDef == null)
                {
                    throw DatabaseExceptions.PropertyNotFound(modelDef.ModelFullName, propertyName);
                }

                Label nullLabel = il.DefineLabel();
                Label finishLabel = il.DefineLabel();

                il.Emit(OpCodes.Ldloc, rtArray);//[rtArray]

                il.Emit(OpCodes.Ldstr, $"{propertyDef.DbParameterizedName!}_");

                il.Emit(OpCodes.Ldloc, parameterSuffixLocal);

                il.EmitCall(OpCodes.Call, CommonReflectionInfos.StringConcatMethod, null);//[rtArray][key]

                il.Emit(OpCodes.Ldloc, propertyValues);//[rtArray][key][propetyValues]

                EmitUtil.EmitInt32(il, index); //[rtArray][key][propetyValues][index]

                il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArrayGetValueMethod, null); //[rtArray][key][property_value_obj(boxed)]

                //if (propertyDef.Type.IsValueType)
                //{
                //    il.EmitCall(OpCodes.Call, propertyDef.GetMethod, null);
                //    il.Emit(OpCodes.Box, propertyDef.Type);
                //}
                //else
                //{
                //    il.EmitCall(OpCodes.Callvirt, propertyDef.GetMethod, null);
                //}

                //[rtArray][key][property_value_obj]

                #region TypeValue To DbValue

                //判断是否是null
                il.Emit(OpCodes.Dup);//[rtArray][key][property_value_obj][property_value_obj]
                il.Emit(OpCodes.Brfalse_S, nullLabel);//[rtArray][key][property_value_obj]

                if (propertyDef.TypeConverter != null)
                {
                    il.Emit(OpCodes.Stloc, tmpObj);//[rtArray][key]

                    il.Emit(OpCodes.Ldarg_0); //[rtArray][key][IDbModelDefFactory]

                    il.Emit(OpCodes.Ldloc, modelTypeLocal);//[rtArray][key][IDbModelDefFactory][modelType]
                    //emiter.LoadLocal(modelTypeLocal);

                    il.Emit(OpCodes.Ldstr, propertyDef.Name);//[rtArray][key][IDbModelDefFactory][modelType][propertyName]
                    il.EmitCall(OpCodes.Callvirt, _getPropertyTypeConverterMethod2, null);//[rtArray][key][typeconverter]

                    il.Emit(OpCodes.Ldloc, tmpObj);//[rtArray][key][typeconveter][property_value_obj]

                    il.Emit(OpCodes.Ldtoken, propertyDef.Type);
                    //emiter.LoadConstant(propertyDef.Type);
                    il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);
                    //emiter.Call(DBModelConverterEmit._getTypeFromHandleMethod);
                    //[rtArray][key][typeconveter][property_value_obj][property_type]
                    il.EmitCall(OpCodes.Callvirt, _getTypeConverterTypeValueToDbValueMethod, null);
                    //emiter.CallVirtual(typeof(IDbPropertyConverter).GetMethod(nameof(IDbPropertyConverter.PropertyValueToDbFieldValue)));
                    //[rtArray][key][db_value]
                }
                else
                {
                    Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

                    //查看全局TypeConvert

                    IDbPropertyConverter? globalConverter = DbPropertyConvert.GetGlobalDbPropertyConverter(trueType, engineType);

                    if (globalConverter != null)
                    {
                        il.Emit(OpCodes.Stloc, tmpObj);
                        //emiter.StoreLocal(tmpObj);
                        //[rtArray][key]

                        il.Emit(OpCodes.Ldtoken, trueType);
                        //emiter.LoadConstant(trueType);
                        il.EmitCall(OpCodes.Call, CommonReflectionInfos.GetTypeFromHandleMethod, null);
                        //emiter.Call(DBModelConverterEmit._getTypeFromHandleMethod);
                        il.Emit(OpCodes.Stloc, tmpTrueTypeLocal);
                        //emiter.StoreLocal(tmpTrueTypeLocal);
                        il.Emit(OpCodes.Ldloc, tmpTrueTypeLocal);
                        //emiter.LoadLocal(tmpTrueTypeLocal);

                        EmitUtil.EmitInt32(il, (int)engineType);
                        //emiter.LoadConstant((int)engineType);
                        il.EmitCall(OpCodes.Call, _getGlobalTypeConverterMethod, null);
                        //emiter.Call(DBModelConverterEmit._getGlobalTypeConverterMethod);
                        //[rtArray][key][typeconverter]

                        il.Emit(OpCodes.Ldloc, tmpObj);
                        //emiter.LoadLocal(tmpObj);
                        //[rtArray][key][typeconverter][property_value_obj]
                        il.Emit(OpCodes.Ldloc, tmpTrueTypeLocal);
                        //emiter.LoadLocal(tmpTrueTypeLocal);
                        //[rtArray][key][typeconverter][property_value_obj][true_type]
                        il.EmitCall(OpCodes.Callvirt, _getTypeConverterTypeValueToDbValueMethod, null);
                        //emiter.CallVirtual(typeof(IDbPropertyConverter).GetMethod(nameof(IDbPropertyConverter.PropertyValueToDbFieldValue)));
                        //[rtArray][key][db_value]
                    }
                    else
                    {
                        //默认
                        if (trueType.IsEnum)
                        {
                            il.EmitCall(OpCodes.Callvirt, CommonReflectionInfos.ObjectToStringMethod, null);
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
                //[rtArray][key][property_value_obj]

                il.Emit(OpCodes.Pop);
                //emiter.Pop();
                //[rtArray][key]

                il.Emit(OpCodes.Ldsfld, CommonReflectionInfos.DbNullValueFiled);
                //emiter.LoadField(typeof(DBNull).GetField("Value"));
                //[rtArray][key][DBNull]

                //il.Emit(OpCodes.Br_S, finishLabel);
                ////emiter.Branch(finishLabel);

                #endregion

                il.MarkLabel(finishLabel);
                ////emiter.MarkLabel(finishLabel);

                var kvCtor = typeof(KeyValuePair<string, object>).GetConstructor(new Type[] { typeof(string), typeof(object) })!;

                il.Emit(OpCodes.Newobj, kvCtor);
                //emiter.NewObject(kvCtor);
                //[rtArray][kv]

                il.Emit(OpCodes.Box, typeof(KeyValuePair<string, object>));
                //emiter.Box<KeyValuePair<string, object>>();
                //[rtArray][kv_obj]

                EmitUtil.EmitInt32(il, index);
                //emiter.LoadConstant(index);
                //[rtArray][kv_obj][index]

                il.EmitCall(OpCodes.Call, CommonReflectionInfos.ArraySetValueMethod, null);
                //emiter.Call(typeof(Array).GetMethod(nameof(Array.SetValue), new Type[] { typeof(object), typeof(int) }));

                index++;
            }

            il.Emit(OpCodes.Ldloc, rtArray);
            //emiter.LoadLocal(rtArray);

            il.Emit(OpCodes.Ret);
            //emiter.Return();

            Type funType = Expression.GetFuncType(typeof(IDbModelDefFactory), typeof(object?[]), typeof(string), typeof(KeyValuePair<string, object>[]));

            return (Func<IDbModelDefFactory, object?[], string, KeyValuePair<string, object>[]>)dm.CreateDelegate(funType);

            //return emiter.CreateDelegate();
        }

        private static readonly MethodInfo _dataReaderGetItemMethod = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)!
            .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))!
            .Select(p => p.GetGetMethod()).First()!;

        private static readonly MethodInfo _getPropertyTypeConverterMethod2 = typeof(IDbModelDefFactory).GetMethod(nameof(IDbModelDefFactory.GetPropertyTypeConverter))!;
        private static readonly MethodInfo _getGlobalTypeConverterMethod = typeof(DbPropertyConvert).GetMethod(nameof(DbPropertyConvert.GetGlobalDbPropertyConverter), new Type[] { typeof(Type), typeof(int) })!;
        private static readonly MethodInfo _getTypeConverterDbValueToTypeValueMethod = typeof(IDbPropertyConverter).GetMethod(nameof(IDbPropertyConverter.DbFieldValueToPropertyValue))!;
        private static readonly MethodInfo _getTypeConverterTypeValueToDbValueMethod = typeof(IDbPropertyConverter).GetMethod(nameof(IDbPropertyConverter.PropertyValueToDbFieldValue))!;
    }
}