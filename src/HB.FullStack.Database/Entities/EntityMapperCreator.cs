using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Xml.Linq;

using HB.FullStack.Database.Entities;

namespace HB.FullStack.Database
{
    internal static class EntityMapperCreator
    {
        public static Func<IDataReader, DatabaseEntityDef, object> CreateEntityMapper(DatabaseEntityDef def, IDataReader reader)
        {
            DynamicMethod dm = new DynamicMethod("Deserialize" + Guid.NewGuid().ToString(), def.EntityType, new[] { typeof(IDataReader), typeof(DatabaseEntityDef) }, def.EntityType, true);
            ILGenerator il = dm.GetILGenerator();

            EmitEntityMapper(def, reader, il);

            var funcType = Expression.GetFuncType(typeof(IDataReader), typeof(DatabaseEntityDef), def.EntityType);
            return (Func<IDataReader, DatabaseEntityDef, object>)dm.CreateDelegate(funcType);
        }

        private static void EmitEntityMapper(DatabaseEntityDef def, IDataReader reader, ILGenerator il)
        {
            try
            {
                List<DatabaseEntityPropertyDef> propertyDefs = new List<DatabaseEntityPropertyDef>();

                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    propertyDefs.Add(def.GetPropertyDef(reader.GetName(i)) ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {reader.GetName(i)}."));
                }

                LocalBuilder returnValueLocal = il.DeclareLocal(def.EntityType);
                LocalBuilder enumStringTempLocal = il.DeclareLocal(typeof(string));
                LocalBuilder timespanZeroLocal = il.DeclareLocal(typeof(TimeSpan));

                ConstructorInfo ctor = def.EntityType.GetDefaultConstructor();

                il.Emit(OpCodes.Newobj, ctor);
                //emitter.NewObject(ctor);
                il.Emit(OpCodes.Stloc, returnValueLocal);
                //emitter.StoreLocal(returnValueLocal);
                il.Emit(OpCodes.Ldloc, returnValueLocal);
                //emitter.LoadLocal(returnValueLocal); // [target]

                for (int index = 0; index < propertyDefs.Count; ++index)
                {
                    Label dbNullLabel = il.DefineLabel();
                    Label finishLable = il.DefineLabel();

                    Type dbValueType = reader.GetFieldType(index);

                    il.Emit(OpCodes.Dup);
                    //emitter.Duplicate(); // stack is now [target][target]

                    DatabaseEntityPropertyDef propertyDef = propertyDefs[index];

                    if (propertyDef.TypeConverter != null)
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        //emitter.LoadArgument(1);// stack is now [target][target][EntityDef]
                        il.Emit(OpCodes.Ldstr, propertyDef.Name);
                        //emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][EntityDef][PropertyName]
                        il.EmitCall(OpCodes.Call, _getPropertyTypeConverterMethod, null);
                        //emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyTypeConverter)));// stack is now [target][target][TypeConverter]
                    }

                    //===获取数据库值=========================================================================

                    il.Emit(OpCodes.Ldarg_0);
                    //emitter.LoadArgument(0);// stack is now [...][reader]
                    EmitInt32(il, index);
                    //emitter.LoadConstant(index); // stack is now [...][reader][index]
                    il.EmitCall(OpCodes.Callvirt, _dataReaderGetItemMethod, null);
                    //emitter.CallVirtual(ReflectionHelper.GetItem);// stack is now [...][value-as-object]

                    //check DBNULL
                    il.Emit(OpCodes.Dup);
                    //emitter.Duplicate(); //stack is now [...][value-as-object][value-as-object]
                    il.Emit(OpCodes.Isinst, typeof(DBNull));
                    //emitter.IsInstance(typeof(DBNull));//stack is now [...][value-as-object][DbNull/null]
                    il.Emit(OpCodes.Brtrue_S, dbNullLabel);
                    //emitter.BranchIfTrue(dbNullLabel);//stack is now [...][value-as-object]


                    //===DbValueToTypeValue,逻辑同DatabaseConverty.DbValueToTypeValue一致======================
                    if (propertyDef.TypeConverter == null)
                    {
                        if (propertyDef.Type.IsEnum || (propertyDef.NullableUnderlyingType != null && propertyDef.NullableUnderlyingType.IsEnum))
                        {
                            // stack is now [target][target][value-as-object]
                            il.Emit(OpCodes.Castclass, typeof(string));
                            //emitter.CastClass<string>(); //stack is now [target][target][string]

                            il.Emit(OpCodes.Stloc, enumStringTempLocal);
                            //emitter.StoreLocal(enumStringTempLocal); //stack is now [target][target]

                            if (propertyDef.NullableUnderlyingType != null)
                            {
                                il.Emit(OpCodes.Ldtoken, propertyDef.NullableUnderlyingType);
                                //emitter.LoadConstant(propertyDef.NullableUnderlyingType);
                            }
                            else
                            {
                                il.Emit(OpCodes.Ldtoken, propertyDef.Type);
                                //emitter.LoadConstant(propertyDef.Type);//stack is now[target][target][propertyType-token]
                            }

                            il.EmitCall(OpCodes.Call, _getTypeFromHandleMethod, null);
                            //emitter.Call(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));//stack is now[target][target][propertyType]

                            il.Emit(OpCodes.Ldloc, enumStringTempLocal);
                            //emitter.LoadLocal(enumStringTempLocal);//stack is now[target][target][propertyType][string]

                            il.Emit(OpCodes.Ldc_I4_1);
                            //emitter.LoadConstant(true);//stack is now[target][target][propertyType][string][true]

                            il.EmitCall(OpCodes.Call, _enumParseMethod, null);
                            //emitter.Call(EnumParse);//stack is now[target][target][value]

                            if (propertyDef.NullableUnderlyingType != null)
                            {
                                il.Emit(OpCodes.Unbox_Any, propertyDef.NullableUnderlyingType);
                                //emitter.UnboxAny(propertyDef.NullableUnderlyingType);
                            }
                            else
                            {
                                il.Emit(OpCodes.Unbox_Any, propertyDef.Type);
                                //emitter.UnboxAny(propertyDef.Type);
                            }

                            //stack is now[target][target][typed-value]
                        }
                        else if (propertyDef.Type == typeof(DateTimeOffset) || (propertyDef.NullableUnderlyingType != null && propertyDef.NullableUnderlyingType == typeof(DateTimeOffset)))
                        {
                            // stack is now [target][target][value-as-object]

                            if (dbValueType == typeof(string))
                            {
                                il.Emit(OpCodes.Castclass, typeof(string));//stack is now [target][target][string]
                                il.EmitCall(OpCodes.Call, _invariantCultureMethod, null);//stack is now [target][target][string][cultureInfo]
                                il.EmitCall(OpCodes.Call, _dataTimeOffsetParseMethod, null);
                                //stack is now [target][target][datetimeoffset]
                            }
                            else
                            {

                                il.Emit(OpCodes.Unbox_Any, typeof(DateTime));
                                //emitter.UnboxAny(typeof(DateTime));//stack is now[target][target][datetime]

                                il.Emit(OpCodes.Ldloc, timespanZeroLocal);
                                //emitter.LoadLocal(timespanZeroLocal); //stack is now[target][target][datetime][timespan.zero]


                                il.Emit(OpCodes.Newobj, _dateTimeOffsetConstructorInfo);
                                //emitter.NewObject(dateTimeOffsetConstructorInfo);
                                //stack is now[target][target][datetimeoffset]
                                //??need unbox
                            }
                        }
                        else
                        {
                            // stack is now [target][target][value-as-object]

                            if (dbValueType == propertyDef.Type || dbValueType == propertyDef.NullableUnderlyingType)
                            {
                                if (propertyDef.NullableUnderlyingType != null)
                                {
                                    il.Emit(OpCodes.Unbox_Any, propertyDef.NullableUnderlyingType);
                                    //emitter.UnboxAny(propertyDef.NullableUnderlyingType);
                                }
                                else
                                {
                                    il.Emit(OpCodes.Unbox_Any, propertyDef.Type);
                                    //emitter.UnboxAny(propertyDef.Type);
                                }
                            }
                            else
                            {
                                FlexibleConvertBoxedFromHeadOfStack(il, dbValueType, propertyDef.NullableUnderlyingType ?? propertyDef.Type);
                            }

                            //stack is now[target][target][typed-value]
                        }

                        if (propertyDef.NullableUnderlyingType != null)
                        {
                            il.Emit(OpCodes.Newobj, propertyDef.Type.GetConstructor(new Type[] { propertyDef.NullableUnderlyingType }));
                            //emitter.NewObject(propertyDef.Type.GetConstructor(new Type[] { propertyDef.NullableUnderlyingType }));
                        }
                    }
                    else
                    {
                        // stack is now [target][target][TypeConverter][value-as-object]
                        il.EmitCall(OpCodes.Callvirt, _typeConverterDbValueToTypeValueMethod, null);
                        //emitter.CallVirtual(typeof(DatabaseTypeConverter).GetMethod(nameof(DatabaseTypeConverter.DbValueToTypeValue)));

                        il.Emit(OpCodes.Unbox_Any, propertyDef.Type);
                        //emitter.UnboxAny(propertyDef.Type);

                        // stack is now [target][target][TypeValue]
                    }

                    //===赋值================================================================================
                    // stack is now [target][target][TypeValue]

                    if (propertyDef.Type.IsValueType)
                    {
                        il.EmitCall(OpCodes.Call, propertyDef.SetMethod, null);
                        //emitter.Call(GetPropertySetter(propertyDef.PropertyInfo, def.EntityType));
                    }
                    else
                    {
                        il.EmitCall(OpCodes.Callvirt, propertyDef.SetMethod, null);
                        //emitter.CallVirtual(GetPropertySetter(propertyDef.PropertyInfo, def.EntityType));
                    }

                    //stack is now[target]
                    il.Emit(OpCodes.Br_S, finishLable);
                    //emitter.Branch(finishLable);

                    il.MarkLabel(dbNullLabel);
                    //emitter.MarkLabel(dbNullLabel);

                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Pop);
                    //emitter.Pop();
                    //emitter.Pop();
                    if (propertyDef.TypeConverter != null)
                    {
                        il.Emit(OpCodes.Pop);
                    }

                    il.MarkLabel(finishLable);
                    //emitter.MarkLabel(finishLable);

                }

                //emitter.StoreLocal(returnValueLocal);

                //emitter.LoadLocal(returnValueLocal);

                il.Emit(OpCodes.Ret);
                //emitter.Return();
            }
            catch (Exception)
            {
                //string info = ex.GetDebugInfo();
                throw;
            }
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
            if (from == (to))
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
                    il.EmitCall(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)), null); // stack is now [target][target][value][member-type]
                    il.EmitCall(OpCodes.Call, _invariantCultureMethod, null); // stack is now [target][target][value][member-type][culture]
                    il.EmitCall(OpCodes.Call, typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[] { typeof(object), typeof(Type), typeof(IFormatProvider) }), null); // stack is now [target][target][boxed-member-type-value]
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

        private static MethodInfo GetPropertySetterMethod(PropertyInfo propertyInfo, Type type)
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

        private static readonly MethodInfo _dataReaderGetItemMethod = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))
            .Select(p => p.GetGetMethod()).First();

        private static readonly MethodInfo _enumParseMethod = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) });

        private static readonly MethodInfo _invariantCultureMethod = typeof(CultureInfo).GetProperty(nameof(CultureInfo.InvariantCulture), BindingFlags.Public | BindingFlags.Static).GetGetMethod();

        private static readonly MethodInfo _getPropertyTypeConverterMethod = typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyTypeConverter));

        private static readonly MethodInfo _getTypeFromHandleMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

        private static readonly MethodInfo _dataTimeOffsetParseMethod = typeof(DateTimeOffset).GetMethod(nameof(DateTimeOffset.Parse), new Type[] { typeof(string), typeof(IFormatProvider) });

        private static readonly ConstructorInfo _dateTimeOffsetConstructorInfo = typeof(DateTimeOffset).GetConstructor(new Type[] { typeof(DateTime), typeof(TimeSpan) });

        private static readonly MethodInfo _typeConverterDbValueToTypeValueMethod = typeof(CustomTypeConverter).GetMethod(nameof(CustomTypeConverter.DbValueToTypeValue));
    }
}
