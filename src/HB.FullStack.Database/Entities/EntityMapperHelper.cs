using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Xml.Linq;

using HB.FullStack.Database.Entities;



namespace HB.FullStack.Database
{
    internal static class EntityMapperHelper
    {
        public static readonly MethodInfo GetItem = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))
        .Select(p => p.GetGetMethod()).First();

        public static readonly MethodInfo EnumParse = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) });

        internal static Func<IDataReader, DatabaseEntityDef, object> CreateEntityMapperDelegate(DatabaseEntityDef def, IDataReader reader)
        {
            var dm = new DynamicMethod("Deserialize" + Guid.NewGuid().ToString(), def.EntityType, new[] { typeof(IDataReader), typeof(DatabaseEntityDef) }, def.EntityType, true);
            var il = dm.GetILGenerator();

            GenerateDeserializerFromMap(def, reader, il);

            var funcType = System.Linq.Expressions.Expression.GetFuncType(typeof(IDataReader), typeof(DatabaseEntityDef), def.EntityType);
            return (Func<IDataReader, DatabaseEntityDef, object>)dm.CreateDelegate(funcType);
        }

        internal static void GenerateDeserializerFromMap(DatabaseEntityDef def, IDataReader reader, ILGenerator il)
        {
            try
            {
                List<DatabaseEntityPropertyDef> propertyDefs = new List<DatabaseEntityPropertyDef>();

                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    propertyDefs.Add(def.GetProperty(reader.GetName(i)) ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {reader.GetName(i)}."));
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

                    il.Emit(OpCodes.Dup);
                    //emitter.Duplicate(); // stack is now [target][target]

                    DatabaseEntityPropertyDef propertyDef = propertyDefs[index];

                    if (propertyDef.TypeConverter != null)
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        //emitter.LoadArgument(1);// stack is now [target][target][EntityDef]
                        il.Emit(OpCodes.Ldstr, propertyDef.PropertyInfo.Name);
                        //emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][EntityDef][PropertyName]

                        il.EmitCall(OpCodes.Call, typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyTypeConverter)), null);
                        //emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyTypeConverter)));// stack is now [target][target][TypeConverter]
                    }

                    ///获取数据库值

                    il.Emit(OpCodes.Ldarg_0);
                    //emitter.LoadArgument(0);// stack is now [...][reader]
                    EmitInt32(il, index);
                    //emitter.LoadConstant(index); // stack is now [...][reader][index]
                    il.EmitCall(OpCodes.Callvirt, GetItem, null);
                    //emitter.CallVirtual(ReflectionHelper.GetItem);// stack is now [...][value-as-object]

                    //check DBNULL
                    il.Emit(OpCodes.Dup);
                    //emitter.Duplicate(); //stack is now [...][value-as-object][value-as-object]
                    il.Emit(OpCodes.Isinst, typeof(DBNull));
                    //emitter.IsInstance(typeof(DBNull));//stack is now [...][value-as-object][DbNull/null]
                    il.Emit(OpCodes.Brtrue_S, dbNullLabel);
                    //emitter.BranchIfTrue(dbNullLabel);//stack is now [...][value-as-object]

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

                            il.EmitCall(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)), null);
                            //emitter.Call(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));//stack is now[target][target][propertyType]

                            il.Emit(OpCodes.Ldloc, enumStringTempLocal);
                            //emitter.LoadLocal(enumStringTempLocal);//stack is now[target][target][propertyType][string]

                            il.Emit(OpCodes.Ldc_I4_1);
                            //emitter.LoadConstant(true);//stack is now[target][target][propertyType][string][true]

                            il.EmitCall(OpCodes.Call, EnumParse, null);
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

                            il.Emit(OpCodes.Unbox_Any, typeof(DateTime));
                            //emitter.UnboxAny(typeof(DateTime));//stack is now[target][target][datetime]

                            il.Emit(OpCodes.Ldloc, timespanZeroLocal);
                            //emitter.LoadLocal(timespanZeroLocal); //stack is now[target][target][datetime][timespan.zero]
                            ConstructorInfo dateTimeOffsetConstructorInfo = typeof(DateTimeOffset).GetConstructor(new Type[] { typeof(DateTime), typeof(TimeSpan) });

                            il.Emit(OpCodes.Newobj, dateTimeOffsetConstructorInfo);
                            //emitter.NewObject(dateTimeOffsetConstructorInfo);
                            //stack is now[target][target][datetimeoffset]
                            //??need unbox
                        }
                        else
                        {
                            // stack is now [target][target][value-as-object]

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

                        if (propertyDef.NullableUnderlyingType != null)
                        {
                            il.Emit(OpCodes.Newobj, propertyDef.Type.GetConstructor(new Type[] { propertyDef.NullableUnderlyingType }));
                            //emitter.NewObject(propertyDef.Type.GetConstructor(new Type[] { propertyDef.NullableUnderlyingType }));
                        }
                    }
                    else
                    {
                        // stack is now [target][target][TypeConverter][value-as-object]
                        il.EmitCall(OpCodes.Callvirt, typeof(DatabaseTypeConverter).GetMethod(nameof(DatabaseTypeConverter.DbValueToTypeValue)), null);
                        //emitter.CallVirtual(typeof(DatabaseTypeConverter).GetMethod(nameof(DatabaseTypeConverter.DbValueToTypeValue)));

                        il.Emit(OpCodes.Unbox_Any, propertyDef.Type);
                        //emitter.UnboxAny(propertyDef.Type);

                        // stack is now [target][target][TypeValue]
                    }

                    // stack is now [target][target][TypeValue]

                    if (propertyDef.Type.IsValueType)
                    {
                        il.EmitCall(OpCodes.Call, GetPropertySetter(propertyDef.PropertyInfo, def.EntityType), null);
                        //emitter.Call(GetPropertySetter(propertyDef.PropertyInfo, def.EntityType));
                    }
                    else
                    {
                        il.EmitCall(OpCodes.Callvirt, GetPropertySetter(propertyDef.PropertyInfo, def.EntityType), null);
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

        internal static MethodInfo GetPropertySetter(PropertyInfo propertyInfo, Type type)
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

        public static void ThrowDataException(Exception ex, int index, IDataReader reader, object value)
        {
            Exception toThrow;
            try
            {
                string name = "(n/a)", formattedValue = "(n/a)";
                if (reader != null && index >= 0 && index < reader.FieldCount)
                {
                    name = reader.GetName(index);
                    if (string.IsNullOrEmpty(name))
                    {
                        // Otherwise we throw (=value) below, which isn't intuitive
                        name = "(Unnamed Column)";
                    }
                    try
                    {
                        if (value == null || value is DBNull)
                        {
                            formattedValue = "<null>";
                        }
                        else
                        {
                            formattedValue = Convert.ToString(value, CultureInfo.InvariantCulture) + " - " + Type.GetTypeCode(value.GetType());
                        }
                    }
                    catch (Exception valEx)
                    {
                        formattedValue = valEx.Message;
                    }
                }
                toThrow = new DataException($"Error parsing column {index} ({name}={formattedValue})", ex);
            }
            catch
            { // throw the **original** exception, wrapped as DataException
                toThrow = new DataException(ex.Message, ex);
            }
            throw toThrow;
        }
    }
}
