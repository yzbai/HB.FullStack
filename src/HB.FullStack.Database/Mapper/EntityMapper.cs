#nullable enable

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.FullStack.Database.Entities
{
    internal static class EntityMapper
    {
        private static readonly ConcurrentDictionary<string, Func<IDataReader, DatabaseEntityDef, object?>> _mapperDict = new ConcurrentDictionary<string, Func<IDataReader, DatabaseEntityDef, object?>>();

        private static readonly object _mapperLocker = new object();

        public static IList<T> ToEntities<T>(this IDataReader reader, DatabaseEntityDef entityDef)
            where T : Entity, new()
        {
            Func<IDataReader, DatabaseEntityDef, object?> mapFunc = GetCachedMapFunc(reader, entityDef, 0, reader.FieldCount, false);

            List<T> lst = new List<T>();

            while (reader.Read())
            {
                object item = mapFunc.Invoke(reader, entityDef)!;

                lst.Add((T)item);
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget?>> ToEntities<TSource, TTarget>(this IDataReader reader, DatabaseEntityDef sourceEntityDef, DatabaseEntityDef targetEntityDef)
            where TSource : Entity, new()
            where TTarget : Entity, new()
        {
            var sourceFunc = GetCachedMapFunc(reader, sourceEntityDef, 0, sourceEntityDef.FieldCount, false);
            var targetFunc = GetCachedMapFunc(reader, targetEntityDef, sourceEntityDef.FieldCount, reader.FieldCount - sourceEntityDef.FieldCount, true);

            IList<Tuple<TSource, TTarget?>> lst = new List<Tuple<TSource, TTarget?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(reader, sourceEntityDef)!;
                object? target = targetFunc.Invoke(reader, targetEntityDef);

                lst.Add(new Tuple<TSource, TTarget?>((TSource)source, (TTarget?)target));
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget2?, TTarget3?>> ToEntities<TSource, TTarget2, TTarget3>(this IDataReader reader, DatabaseEntityDef sourceEntityDef, DatabaseEntityDef targetEntityDef1, DatabaseEntityDef targetEntityDef2)
            where TSource : Entity, new()
            where TTarget2 : Entity, new()
            where TTarget3 : Entity, new()
        {
            var sourceFunc = GetCachedMapFunc(reader, sourceEntityDef, 0, sourceEntityDef.FieldCount, false);
            var targetFunc1 = GetCachedMapFunc(reader, targetEntityDef1, sourceEntityDef.FieldCount, targetEntityDef1.FieldCount, true);
            var targetFunc2 = GetCachedMapFunc(reader, targetEntityDef2, sourceEntityDef.FieldCount + targetEntityDef1.FieldCount, reader.FieldCount - (sourceEntityDef.FieldCount + targetEntityDef1.FieldCount), true);

            IList<Tuple<TSource, TTarget2?, TTarget3?>> lst = new List<Tuple<TSource, TTarget2?, TTarget3?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(reader, sourceEntityDef)!;
                object? target1 = targetFunc1.Invoke(reader, targetEntityDef1);
                object? target2 = targetFunc2.Invoke(reader, targetEntityDef2);

                lst.Add(new Tuple<TSource, TTarget2?, TTarget3?>((TSource)source, (TTarget2?)target1, (TTarget3?)target2));
            }

            return lst;
        }

        private static Func<IDataReader, DatabaseEntityDef, object?> GetCachedMapFunc(IDataReader reader, DatabaseEntityDef entityDef, int startIndex, int length, bool returnNullIfFirstNull)
        {
            string key = GetKey(entityDef, startIndex, length, returnNullIfFirstNull);

            if (!_mapperDict.ContainsKey(key))
            {
                lock (_mapperLocker)
                {
                    if (!_mapperDict.ContainsKey(key))
                    {
                        _mapperDict[key] = EntityMapperCreator.CreateEntityMapper(entityDef, reader, startIndex, length, returnNullIfFirstNull);
                    }
                }
            }

            return _mapperDict[key];

            static string GetKey(DatabaseEntityDef entityDef, int startIndex, int length, bool returnNullIfFirstNull)
            {
                return $"{entityDef.DatabaseName}_{entityDef.TableName}_{startIndex}_{length}_{returnNullIfFirstNull}";
            }
        }

        //TODO: 优化
        public static IList<KeyValuePair<string, object>> ToParameters<T>(this T entity, DatabaseEntityDef entityDef, int number = 0) where T : Entity, new()
        {
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(entityDef.FieldCount);

            foreach (var propertyDef in entityDef.PropertyDefs)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName!}_{number}",
                    TypeConverter.TypeValueToDbValue(propertyDef.GetValueFrom(entity), propertyDef)));
            }

            return parameters;
        }
    }
}
