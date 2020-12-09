using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

using HB.FullStack.Database.Entities;

using Sigil;

namespace HB.FullStack.Database
{
    internal static class EntityMapperHelper2
    {
        internal static Func<IDataReader, DatabaseEntityDef, object> CreateEntityMapperDelegate(DatabaseEntityDef def, IDataReader reader)
        {
            //var returnType = def.EntityType;

            var emitter = Emit<Func<IDataReader, DatabaseEntityDef, object>>.NewDynamicMethod("Deserialize" + Guid.NewGuid().ToString());

            GenerateDeserializerFromMap(def, reader, emitter);


            return emitter.CreateDelegate();
            //var funcType = System.Linq.Expressions.Expression.GetFuncType(typeof(IDataReader), typeof(DatabaseEntityDef), returnType);
            //return (Func<IDataReader, DatabaseEntityDef, object>)dm.CreateDelegate(funcType);
        }

        internal static void GenerateDeserializerFromMap(DatabaseEntityDef def, IDataReader reader, Emit<Func<IDataReader, DatabaseEntityDef, object>> emitter)
        {
            try
            {
                List<DatabaseEntityPropertyDef> propertyDefs = new List<DatabaseEntityPropertyDef>();

                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    propertyDefs.Add(def.GetProperty(reader.GetName(i)) ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {reader.GetName(i)}."));
                }

                Local returnValueLocal = emitter.DeclareLocal(def.EntityType);

                ConstructorInfo ctor = def.EntityType.GetDefaultConstructor();

                emitter.NewObject(ctor);
                emitter.StoreLocal(returnValueLocal);

                emitter.LoadLocal(returnValueLocal); // [target]

                int index = 0;

                foreach (DatabaseEntityPropertyDef propertyDef in propertyDefs)
                {
                    emitter.Duplicate(); // stack is now [target][target]

                    if (propertyDef.TypeConverter != null)
                    {
                        emitter.LoadArgument(1);// stack is now [target][target][EntityDef]
                        emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][EntityDef][PropertyName]
                        emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyTypeConverter)));// stack is now [target][target][TypeConverter]
                    }

                    emitter.LoadArgument(0);// stack is now [...][reader]
                    emitter.LoadConstant(index); // stack is now [...][reader][index]
                    emitter.CallVirtual(ReflectionHelper.GetItem);// stack is now [...][value-as-object]

                    if (propertyDef.TypeConverter == null)
                    {
                        // stack is now [target][target][value-as-object]
                        emitter.LoadArgument(1);// stack is now [target][target][value-as-object][EntityDef]
                        emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][value-as-object][EntityDef][PropertyName]
                        emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyType)));// stack is now [target][target][value-as-object][PropertyType]
                        emitter.Call(typeof(ValueConverterUtil).GetMethod(nameof(ValueConverterUtil.DbValueToTypeValue)));// stack is now [target][target][TypeValue]
                    }
                    else
                    {
                        // stack is now [target][target][TypeConverter][value-as-object]
                        emitter.CallVirtual(typeof(DatabaseTypeConverter).GetMethod(nameof(DatabaseTypeConverter.DbValueToTypeValue)));
                        // stack is now [target][target][TypeValue]
                    }

                    // stack is now [target][target][TypeValue]

                    emitter.UnboxAny(propertyDef.Type);

                    if (propertyDef.Type.IsValueType)
                    {
                        emitter.Call(GetPropertySetter(propertyDef.PropertyInfo, def.EntityType));
                    }
                    else
                    {
                        emitter.CallVirtual(GetPropertySetter(propertyDef.PropertyInfo, def.EntityType));
                    }

                    //emitter.LoadArgument(1);// stack is now [target][target][TypeValue][EntityDef]
                    //emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][TypeValue][EntityDef][PropertyName]
                    //emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertySetMethod)));// stack is now [target][target][TypeValue][SetMethod]
                    //emitter.Call(typeof(ReflectionHelper).GetMethod(nameof(ReflectionHelper.SetPropertyValue))); //stack is now[target]

                    index++;
                }

                emitter.StoreLocal(returnValueLocal);

                emitter.LoadLocal(returnValueLocal);

                emitter.Return();
            }
            catch (SigilVerificationException ex)
            {
                string info = ex.GetDebugInfo();
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
