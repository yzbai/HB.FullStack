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
        public static readonly MethodInfo GetItem = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))
        .Select(p => p.GetGetMethod()).First();

        public static readonly MethodInfo EnumParse = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) });

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
                Local enumStringTempLocal = emitter.DeclareLocal(typeof(string));
                Local timespanZeroLocal = emitter.DeclareLocal(typeof(TimeSpan));

                ConstructorInfo ctor = def.EntityType.GetDefaultConstructor();

                emitter.NewObject(ctor);
                emitter.StoreLocal(returnValueLocal);

                emitter.LoadLocal(returnValueLocal); // [target]

                for (int index = 0; index < propertyDefs.Count; ++index)
                {
                    Label dbNullLabel = emitter.DefineLabel("nul_" + index);
                    Label finishLable = emitter.DefineLabel("fi_" + index);

                    emitter.Duplicate(); // stack is now [target][target]

                    DatabaseEntityPropertyDef propertyDef = propertyDefs[index];

                    if (propertyDef.TypeConverter != null)
                    {
                        emitter.LoadArgument(1);// stack is now [target][target][EntityDef]
                        emitter.LoadConstant(propertyDef.PropertyInfo.Name);// stack is now [target][target][EntityDef][PropertyName]
                        emitter.Call(typeof(DatabaseEntityDef).GetMethod(nameof(DatabaseEntityDef.OnlyForEmitGetPropertyTypeConverter)));
                        // stack is now [target][target][TypeConverter]
                    }

                    emitter.LoadArgument(0);// stack is now [...][reader]
                    emitter.LoadConstant(index); // stack is now [...][reader][index]
                    emitter.CallVirtual(GetItem);// stack is now [...][value-as-object]

                    //check DBNULL
                    emitter.Duplicate(); //stack is now [...][value-as-object][value-as-object]
                    emitter.IsInstance(typeof(DBNull));//stack is now [...][value-as-object][DbNull/null]
                    emitter.BranchIfTrue(dbNullLabel);//stack is now [...][value-as-object]
                    //TODO: sigil版本的老出错，原生版本没问题，就这里的分支

                    if (propertyDef.TypeConverter == null)
                    {
                        if (propertyDef.Type.IsEnum || (propertyDef.NullableUnderlyingType != null && propertyDef.NullableUnderlyingType.IsEnum))
                        {
                            // stack is now [target][target][value-as-object]

                            emitter.CastClass<string>(); //stack is now [target][target][string]
                            emitter.StoreLocal(enumStringTempLocal); //stack is now [target][target]

                            if (propertyDef.NullableUnderlyingType != null)
                            {
                                emitter.LoadConstant(propertyDef.NullableUnderlyingType);
                            }
                            else
                            {
                                emitter.LoadConstant(propertyDef.Type);//stack is now[target][target][propertyType-token]

                            }
                            emitter.Call(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));//stack is now[target][target][propertyType]
                            emitter.LoadLocal(enumStringTempLocal);//stack is now[target][target][propertyType][string]
                            emitter.LoadConstant(true);//stack is now[target][target][propertyType][string][true]
                            emitter.Call(EnumParse);//stack is now[target][target][value]

                            if (propertyDef.NullableUnderlyingType != null)
                            {
                                emitter.UnboxAny(propertyDef.NullableUnderlyingType);
                            }
                            else
                            {
                                emitter.UnboxAny(propertyDef.Type);
                            }

                            //stack is now[target][target][typed-value]
                        }
                        else if (propertyDef.Type == typeof(DateTimeOffset) || (propertyDef.NullableUnderlyingType != null && propertyDef.NullableUnderlyingType == typeof(DateTimeOffset)))
                        {
                            // stack is now [target][target][value-as-object]


                            emitter.UnboxAny(typeof(DateTime));//stack is now[target][target][datetime]
                            emitter.LoadLocal(timespanZeroLocal); //stack is now[target][target][datetime][timespan.zero]
                            ConstructorInfo dateTimeOffsetConstructorInfo = typeof(DateTimeOffset).GetConstructor(new Type[] { typeof(DateTime), typeof(TimeSpan) });
                            emitter.NewObject(dateTimeOffsetConstructorInfo);
                            //stack is now[target][target][datetimeoffset]
                            //??need unbox
                        }
                        else
                        {
                            // stack is now [target][target][value-as-object]


                            if (propertyDef.NullableUnderlyingType != null)
                            {
                                emitter.UnboxAny(propertyDef.NullableUnderlyingType);
                            }
                            else
                            {
                                emitter.UnboxAny(propertyDef.Type);
                            }

                            //stack is now[target][target][typed-value]
                        }

                        if (propertyDef.NullableUnderlyingType != null)
                        {
                            //emitter.UnboxAny(propertyDef.NullableUnderlyingType);
                            emitter.NewObject(propertyDef.Type.GetConstructor(new Type[] { propertyDef.NullableUnderlyingType }));
                        }
                    }
                    else
                    {
                        // stack is now [target][target][TypeConverter][value-as-object]
                        emitter.CallVirtual(typeof(DatabaseTypeConverter).GetMethod(nameof(DatabaseTypeConverter.DbValueToTypeValue)));
                        // stack is now [target][target][TypeValue]

                        emitter.UnboxAny(propertyDef.Type);
                    }

                    // stack is now [target][target][TypeValue]

                    if (propertyDef.Type.IsValueType)
                    {
                        emitter.Call(GetPropertySetter(propertyDef.PropertyInfo, def.EntityType));
                    }
                    else
                    {
                        emitter.CallVirtual(GetPropertySetter(propertyDef.PropertyInfo, def.EntityType));
                    }

                    //stack is now[target]
                    emitter.Branch(finishLable);

                    emitter.MarkLabel(dbNullLabel);

                    emitter.Pop();
                    emitter.Pop();
                    if (propertyDef.TypeConverter != null)
                    {
                        emitter.Pop();
                    }

                    emitter.MarkLabel(finishLable);
                }

                //emitter.StoreLocal(returnValueLocal);

                //emitter.LoadLocal(returnValueLocal);

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


    }
}
