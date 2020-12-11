#nullable enable

using HB.FullStack.Common.Entities;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace HB.FullStack.Database.Entities
{
    /// <summary>
    /// 单例
    /// </summary>
    internal class DefaultDatabaseEntityMapper : IDatabaseEntityMapper
    {
        private readonly IDatabaseEntityDefFactory _entityDefFactory;

        private readonly ConcurrentDictionary<string, Func<IDataReader, DatabaseEntityDef, object>> _mapperDict = new ConcurrentDictionary<string, Func<IDataReader, DatabaseEntityDef, object>>();
        private readonly object _mapperLocker = new object();

        public DefaultDatabaseEntityMapper(IDatabaseEntityDefFactory modelDefFactory)
        {
            _entityDefFactory = modelDefFactory;
        }

        public IList<T> ToList<T>(DatabaseEntityDef entityDef, IDataReader reader)
            where T : Entity, new()
        {
            Func<IDataReader, DatabaseEntityDef, object> mapFunc = GetCachedMapFunc(entityDef, reader);

            List<T> lst = new List<T>();

            while (reader.Read())
            {
                object item = mapFunc.Invoke(reader, entityDef);

                lst.Add((T)item);
            }

            return lst;
        }

        public IList<Tuple<TSource, TTarget?>> ToList<TSource, TTarget>(IDataReader reader)
            where TSource : Entity, new()
            where TTarget : Entity, new()
        {
            IList<Tuple<TSource, TTarget?>> lst = new List<Tuple<TSource, TTarget?>>();

            //if (reader == null)
            //{
            //    return lst;
            //}

            DatabaseEntityDef definition1 = _entityDefFactory.GetDef<TSource>();
            DatabaseEntityDef definition2 = _entityDefFactory.GetDef<TTarget>();

            string[] propertyNames1 = new string[definition1.FieldCount];
            string[] propertyNames2 = new string[definition2.FieldCount];

            int j = 0;

            for (int i = 0; i < definition1.FieldCount; ++j, ++i)
            {
                propertyNames1[i] = reader.GetName(j);
            }

            for (int i = 0; i < definition2.FieldCount; ++j, ++i)
            {
                propertyNames2[i] = reader.GetName(j);
            }

            while (reader.Read())
            {
                TSource t1 = new TSource();
                TTarget? t2 = new TTarget();

                j = 0;

                for (int i = 0; i < definition1.FieldCount; ++i, ++j)
                {
                    DatabaseEntityPropertyDef pDef = definition1.GetPropertyDef(propertyNames1[i])
                        ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {propertyNames1[i]}.");

                    object fieldValue = reader[j];

                    if (pDef.PropertyInfo.Name == "Id" && fieldValue == DBNull.Value)
                    {
                        //TSource 不可以为null
                        //t1 = null;
                        //break;
                        throw new DatabaseException($"Database value of Property 'Id' is null. Entity:{definition1.EntityFullName}");
                    }

                    if (pDef != null)
                    {
                        object? value = pDef.TypeConverter == null ?
                            DatabaseTypeConverter.DbValueToTypeValue(fieldValue, pDef.PropertyInfo.PropertyType) :
                            pDef.TypeConverter.DbValueToTypeValue(fieldValue);

                        if (value != null)
                        {
                            pDef.PropertyInfo.SetValue(t1, value);
                        }
                    }
                }

                for (int i = 0; i < definition2.FieldCount; ++i, ++j)
                {
                    DatabaseEntityPropertyDef pDef = definition2.GetPropertyDef(propertyNames2[i])
                        ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {propertyNames2[i]}."); ;

                    object fieldValue = reader[j];

                    if (pDef.PropertyInfo.Name == "Id" && fieldValue == DBNull.Value)
                    {
                        t2 = null;
                        break;
                        //throw new DatabaseException($"Database value of Property 'Id' is null. Entity:{definition2.EntityFullName}");
                    }

                    if (pDef != null)
                    {
                        object? value = pDef.TypeConverter == null ?
                            DatabaseTypeConverter.DbValueToTypeValue(fieldValue, pDef.PropertyInfo.PropertyType) :
                            pDef.TypeConverter.DbValueToTypeValue(fieldValue);

                        if (value != null)
                        {
                            pDef.PropertyInfo.SetValue(t2, value);
                        }
                    }
                }

                //if (t1 != null && t1.Deleted)
                //{
                //    t1 = null;
                //}
                //if (t2 != null && t2.Deleted)
                //{
                //    t2 = null;
                //}

                //删除全为空
                //if (t1 != null || t2 != null)
                //{
                //    lst.Add(new Tuple<TSource, TTarget>(t1, t2));
                //}

                lst.Add(new Tuple<TSource, TTarget?>(t1, t2));
            }

            return lst;
        }

        /// <summary>
        /// ToList
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">Ignore.</exception>

        public IList<Tuple<TSource, TTarget2?, TTarget3?>> ToList<TSource, TTarget2, TTarget3>(IDataReader reader)
            where TSource : Entity, new()
            where TTarget2 : Entity, new()
            where TTarget3 : Entity, new()
        {
            IList<Tuple<TSource, TTarget2?, TTarget3?>> lst = new List<Tuple<TSource, TTarget2?, TTarget3?>>();

            //if (reader == null)
            //{
            //    return lst;
            //}

            DatabaseEntityDef definition1 = _entityDefFactory.GetDef<TSource>();
            DatabaseEntityDef definition2 = _entityDefFactory.GetDef<TTarget2>();
            DatabaseEntityDef definition3 = _entityDefFactory.GetDef<TTarget3>();

            string[] propertyNames1 = new string[definition1.FieldCount];
            string[] propertyNames2 = new string[definition2.FieldCount];
            string[] propertyNames3 = new string[definition3.FieldCount];

            int j = 0;

            for (int i = 0; i < definition1.FieldCount; ++i, ++j)
            {
                propertyNames1[i] = reader.GetName(j);
            }

            for (int i = 0; i < definition2.FieldCount; ++i, ++j)
            {
                propertyNames2[i] = reader.GetName(j);
            }

            for (int i = 0; i < definition3.FieldCount; ++i, ++j)
            {
                propertyNames3[i] = reader.GetName(j);
            }

            while (reader.Read())
            {
                TSource t1 = new TSource();
                TTarget2? t2 = new TTarget2();
                TTarget3? t3 = new TTarget3();

                j = 0;

                for (int i = 0; i < definition1.FieldCount; ++i, ++j)
                {
                    DatabaseEntityPropertyDef pDef = definition1.GetPropertyDef(propertyNames1[i])
                        ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {propertyNames1[i]}.");

                    object fieldValue = reader[j];

                    if (pDef.PropertyInfo.Name == "Id" && fieldValue == DBNull.Value)
                    {
                        //t1 = null;
                        //break;
                        throw new DatabaseException($"Database value of Property 'Id' is null. Entity:{definition1.EntityFullName}");
                    }

                    if (pDef != null)
                    {
                        object? value = pDef.TypeConverter == null ?
                            DatabaseTypeConverter.DbValueToTypeValue(fieldValue, pDef) :
                            pDef.TypeConverter.DbValueToTypeValue(fieldValue);

                        if (value != null)
                        {
                            pDef.PropertyInfo.SetValue(t1, value);
                        }
                    }
                }

                for (int i = 0; i < definition2.FieldCount; ++i, ++j)
                {
                    DatabaseEntityPropertyDef pDef = definition2.GetPropertyDef(propertyNames2[i])
                        ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {propertyNames2[i]}.");

                    object fieldValue = reader[j];

                    if (pDef.PropertyInfo.Name == "Id" && fieldValue == DBNull.Value)
                    {
                        t2 = null;
                        break;
                    }

                    if (pDef != null)
                    {
                        object? value = pDef.TypeConverter == null ?
                            DatabaseTypeConverter.DbValueToTypeValue(fieldValue, pDef.PropertyInfo.PropertyType) :
                            pDef.TypeConverter.DbValueToTypeValue(fieldValue);

                        if (value != null)
                        {
                            pDef.PropertyInfo.SetValue(t2, value);
                        }
                    }
                }

                for (int i = 0; i < definition3.FieldCount; ++i, ++j)
                {
                    DatabaseEntityPropertyDef pDef = definition3.GetPropertyDef(propertyNames3[i])
                        ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {propertyNames2[i]}.");

                    object fieldValue = reader[j];

                    if (pDef.PropertyInfo.Name == "Id" && fieldValue == DBNull.Value)
                    {
                        t3 = null;
                        break;
                    }

                    if (pDef != null)
                    {
                        object? value = pDef.TypeConverter == null ?
                            DatabaseTypeConverter.DbValueToTypeValue(fieldValue, pDef.PropertyInfo.PropertyType) :
                            pDef.TypeConverter.DbValueToTypeValue(fieldValue);

                        if (value != null)
                        {
                            pDef.PropertyInfo.SetValue(t3, value);
                        }
                    }
                }

                //if (t1 != null && t1.Deleted)
                //{
                //    t1 = null;
                //}

                //if (t2 != null && t2.Deleted)
                //{
                //    t2 = null;
                //}

                //if (t3 != null && t3.Deleted)
                //{
                //    t3 = null;
                //}

                //if (t1 != null || t2 != null || t3 != null)
                //{
                //    lst.Add(new Tuple<TSource, TTarget2, TTarget3>(t1, t2, t3));
                //}

                lst.Add(new Tuple<TSource, TTarget2?, TTarget3?>(t1, t2, t3));
            }

            return lst;
        }

        public void ToObject<T>(IDataReader reader, T item) where T : Entity, new()
        {
            if (reader == null)
            {
                return;
            }

            int len = reader.FieldCount;
            string[] propertyNames = new string[len];

            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();

            for (int i = 0; i < len; ++i)
            {
                propertyNames[i] = reader.GetName(i);
            }

            if (reader.Read())
            {
                for (int i = 0; i < len; ++i)
                {
                    DatabaseEntityPropertyDef property = definition.GetPropertyDef(propertyNames[i])
                        ?? throw new DatabaseException($"Lack DatabaseEntityPropertyDef of {propertyNames[i]}.");

                    object? value = property.TypeConverter == null ?
                        DatabaseTypeConverter.DbValueToTypeValue(reader[i], property.PropertyInfo.PropertyType) :
                        property.TypeConverter.DbValueToTypeValue(reader[i]);

                    if (value != null)
                    {
                        property.PropertyInfo.SetValue(item, value);
                    }
                }
            }
        }

        private Func<IDataReader, DatabaseEntityDef, object> GetCachedMapFunc(DatabaseEntityDef entityDef, IDataReader reader)
        {
            string key = GetKey(entityDef);

            if (!_mapperDict.ContainsKey(key))
            {
                lock (_mapperLocker)
                {
                    if (!_mapperDict.ContainsKey(key))
                    {
                        _mapperDict[key] = EntityMapperCreator.CreateEntityMapper(entityDef, reader);
                    }
                }
            }

            return _mapperDict[key];

            static string GetKey(DatabaseEntityDef entityDef)
            {
                return entityDef.DatabaseName + entityDef.EntityFullName;
            }
        }

#endregion
    }
}
