#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Def;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Mapper
{
    internal static class EntityMapper
    {
        private static readonly ConcurrentDictionary<string, Func<IDataReader, object?>> _mapperDict = new ConcurrentDictionary<string, Func<IDataReader, object?>>();

        private static readonly object _mapperLocker = new object();

        public static IList<T> ToEntities<T>(this IDataReader reader, DatabaseEngineType engineType, EntityDef entityDef)
            where T : Entity, new()
        {
            Func<IDataReader, object?> mapFunc = GetCachedMapFunc(reader, entityDef, 0, reader.FieldCount, false, engineType);

            List<T> lst = new List<T>();

            while (reader.Read())
            {
                object item = mapFunc.Invoke(reader)!;

                lst.Add((T)item);
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget?>> ToEntities<TSource, TTarget>(this IDataReader reader, DatabaseEngineType engineType, EntityDef sourceEntityDef, EntityDef targetEntityDef)
            where TSource : Entity, new()
            where TTarget : Entity, new()
        {
            var sourceFunc = GetCachedMapFunc(reader, sourceEntityDef, 0, sourceEntityDef.FieldCount, false, engineType);
            var targetFunc = GetCachedMapFunc(reader, targetEntityDef, sourceEntityDef.FieldCount, reader.FieldCount - sourceEntityDef.FieldCount, true, engineType);

            IList<Tuple<TSource, TTarget?>> lst = new List<Tuple<TSource, TTarget?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(reader)!;
                object? target = targetFunc.Invoke(reader);

                lst.Add(new Tuple<TSource, TTarget?>((TSource)source, (TTarget?)target));
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget2?, TTarget3?>> ToEntities<TSource, TTarget2, TTarget3>(this IDataReader reader, DatabaseEngineType engineType, EntityDef sourceEntityDef, EntityDef targetEntityDef1, EntityDef targetEntityDef2)
            where TSource : Entity, new()
            where TTarget2 : Entity, new()
            where TTarget3 : Entity, new()
        {
            var sourceFunc = GetCachedMapFunc(reader, sourceEntityDef, 0, sourceEntityDef.FieldCount, false, engineType);
            var targetFunc1 = GetCachedMapFunc(reader, targetEntityDef1, sourceEntityDef.FieldCount, targetEntityDef1.FieldCount, true, engineType);
            var targetFunc2 = GetCachedMapFunc(reader, targetEntityDef2, sourceEntityDef.FieldCount + targetEntityDef1.FieldCount, reader.FieldCount - (sourceEntityDef.FieldCount + targetEntityDef1.FieldCount), true, engineType);

            IList<Tuple<TSource, TTarget2?, TTarget3?>> lst = new List<Tuple<TSource, TTarget2?, TTarget3?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(reader)!;
                object? target1 = targetFunc1.Invoke(reader);
                object? target2 = targetFunc2.Invoke(reader);

                lst.Add(new Tuple<TSource, TTarget2?, TTarget3?>((TSource)source, (TTarget2?)target1, (TTarget3?)target2));
            }

            return lst;
        }

        private static Func<IDataReader, object?> GetCachedMapFunc(IDataReader reader, EntityDef entityDef, int startIndex, int length, bool returnNullIfFirstNull, DatabaseEngineType engineType)
        {
            string key = GetKey(entityDef, startIndex, length, returnNullIfFirstNull, engineType);

            if (!_mapperDict.ContainsKey(key))
            {
                lock (_mapperLocker)
                {
                    if (!_mapperDict.ContainsKey(key))
                    {
                        _mapperDict[key] = EntityMapperCreator.CreateEntityMapper(entityDef, reader, startIndex, length, returnNullIfFirstNull, engineType);
                    }
                }
            }

            return _mapperDict[key];

            static string GetKey(EntityDef entityDef, int startIndex, int length, bool returnNullIfFirstNull, DatabaseEngineType engineType)
            {
                return $"{engineType}_{entityDef.DatabaseName}_{entityDef.TableName}_{startIndex}_{length}_{returnNullIfFirstNull}";
            }
        }

        //TODO: 优化
        public static IList<KeyValuePair<string, object>> ToParameters<T>(this T entity, EntityDef entityDef, DatabaseEngineType engineType, int number = 0) where T : Entity, new()
        {
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(entityDef.FieldCount);

            foreach (var propertyDef in entityDef.PropertyDefs)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName!}_{number}",
                    TypeConvert.TypeValueToDbValue(propertyDef.GetValueFrom(entity), propertyDef, engineType)));
            }

            return parameters;
        }
    }
}