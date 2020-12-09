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
    internal static class EntityMapperHelper3
    {
        public static readonly MethodInfo GetItem = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))
        .Select(p => p.GetGetMethod()).First();

        internal static Func<IDataReader, DatabaseEntityDef, object> CreateEntityMapperDelegate(DatabaseEntityDef def, IDataReader reader)
        {
            //var returnType = def.EntityType;

            var dm = new DynamicMethod("Deserialize" + Guid.NewGuid().ToString(), def.EntityType, new[] { typeof(IDataReader), typeof(DatabaseEntityDef) }, def.EntityType, true);
            var il = dm.GetILGenerator();



            GenerateDeserializerFromMap(def, reader, il);


            var funcType = System.Linq.Expressions.Expression.GetFuncType(typeof(IDataReader), typeof(DatabaseEntityDef), def.EntityType);
            return (Func<IDataReader, DatabaseEntityDef, object>)dm.CreateDelegate(funcType);



            //var emitter = Emit<Func<IDataReader, DatabaseEntityDef, object>>.NewDynamicMethod("Deserialize" + Guid.NewGuid().ToString());

            //GenerateDeserializerFromMap(def, reader, emitter);


            //return emitter.CreateDelegate();
            //var funcType = System.Linq.Expressions.Expression.GetFuncType(typeof(IDataReader), typeof(DatabaseEntityDef), returnType);
            //return (Func<IDataReader, DatabaseEntityDef, object>)dm.CreateDelegate(funcType);
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

                ConstructorInfo ctor = def.EntityType.GetDefaultConstructor();

                il.Emit(OpCodes.Newobj, ctor);
                //emitter.NewObject(ctor);
                il.Emit(OpCodes.Stloc, returnValueLocal);
                //emitter.StoreLocal(returnValueLocal);

                il.Emit(OpCodes.Ldloc, returnValueLocal);
                //emitter.LoadLocal(returnValueLocal); // [target]

                int index = 0;

                foreach (DatabaseEntityPropertyDef propertyDef in propertyDefs)
                {
                    il.Emit(OpCodes.Dup);
                    //emitter.Duplicate(); // stack is now [target][target]

                    if (propertyDef.TypeConverter != null)
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        //emitter.LoadArgument(1);// stack is now [target][target][EntityDef]
                        il.Emit(OpCodes.Ldstr, propertyDef.PropertyInfo.Name);
                        //emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][EntityDef][PropertyName]

                        il.EmitCall(OpCodes.Call, typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyTypeConverter)), null);
                        //emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyTypeConverter)));// stack is now [target][target][TypeConverter]
                    }

                    il.Emit(OpCodes.Ldarg_0);
                    //emitter.LoadArgument(0);// stack is now [...][reader]
                    EmitInt32(il, index);
                    //emitter.LoadConstant(index); // stack is now [...][reader][index]
                    il.EmitCall(OpCodes.Callvirt, GetItem, null);
                    //emitter.CallVirtual(ReflectionHelper.GetItem);// stack is now [...][value-as-object]

                    if (propertyDef.TypeConverter == null)
                    {
                        // stack is now [target][target][value-as-object]
                        il.Emit(OpCodes.Ldarg_1);
                        //emitter.LoadArgument(1);// stack is now [target][target][value-as-object][EntityDef]
                        il.Emit(OpCodes.Ldstr, propertyDef.PropertyInfo.Name);
                        //emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][value-as-object][EntityDef][PropertyName]
                        il.EmitCall(OpCodes.Call, typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyType)), null);
                        //emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyType)));// stack is now [target][target][value-as-object][PropertyType]
                        il.EmitCall(OpCodes.Call, typeof(ValueConverterUtil).GetMethod(nameof(ValueConverterUtil.DbValueToTypeValue)), null);
                        //emitter.Call(typeof(ValueConverterUtil).GetMethod(nameof(ValueConverterUtil.DbValueToTypeValue)));// stack is now [target][target][TypeValue]
                    }
                    else
                    {
                        // stack is now [target][target][TypeConverter][value-as-object]
                        il.EmitCall(OpCodes.Callvirt, typeof(DatabaseTypeConverter).GetMethod(nameof(DatabaseTypeConverter.DbValueToTypeValue)), null);
                        //emitter.CallVirtual(typeof(DatabaseTypeConverter).GetMethod(nameof(DatabaseTypeConverter.DbValueToTypeValue)));
                        // stack is now [target][target][TypeValue]
                    }

                    // stack is now [target][target][TypeValue]

                    il.Emit(OpCodes.Unbox_Any, propertyDef.Type);
                    //emitter.UnboxAny(propertyDef.Type);

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

                    //emitter.LoadArgument(1);// stack is now [target][target][TypeValue][EntityDef]
                    //emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][TypeValue][EntityDef][PropertyName]
                    //emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertySetMethod)));// stack is now [target][target][TypeValue][SetMethod]
                    //emitter.Call(typeof(ReflectionHelper).GetMethod(nameof(ReflectionHelper.SetPropertyValue))); //stack is now[target]

                    index++;
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
