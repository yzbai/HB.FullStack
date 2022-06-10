

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Mapper
{
    internal static class EntityMapper
    {
        #region ToEntity

        private static readonly ConcurrentDictionary<string, Func<IEntityDefFactory, IDataReader, object?>> _toEntityFuncDict = new ConcurrentDictionary<string, Func<IEntityDefFactory, IDataReader, object?>>();

        public static IList<T> ToEntities<T>(this IDataReader reader, EngineType engineType, IEntityDefFactory entityDefFactory, EntityDef entityDef)
            where T : DatabaseEntity, new()
        {
            Func<IEntityDefFactory, IDataReader, object?> mapFunc = GetCachedToEntityFunc(reader, entityDef, 0, reader.FieldCount, false, engineType);

            List<T> lst = new List<T>();

            while (reader.Read())
            {
                object item = mapFunc.Invoke(entityDefFactory, reader)!;

                lst.Add((T)item);
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget?>> ToEntities<TSource, TTarget>(this IDataReader reader, EngineType engineType, IEntityDefFactory entityDefFactory, EntityDef sourceEntityDef, EntityDef targetEntityDef)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            Func<IEntityDefFactory, IDataReader, object?> sourceFunc = GetCachedToEntityFunc(reader, sourceEntityDef, 0, sourceEntityDef.FieldCount, false, engineType);
            Func<IEntityDefFactory, IDataReader, object?> targetFunc = GetCachedToEntityFunc(reader, targetEntityDef, sourceEntityDef.FieldCount, reader.FieldCount - sourceEntityDef.FieldCount, true, engineType);

            IList<Tuple<TSource, TTarget?>> lst = new List<Tuple<TSource, TTarget?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(entityDefFactory, reader)!;
                object? target = targetFunc.Invoke(entityDefFactory, reader);

                lst.Add(new Tuple<TSource, TTarget?>((TSource)source, (TTarget?)target));
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget2?, TTarget3?>> ToEntities<TSource, TTarget2, TTarget3>(this IDataReader reader, EngineType engineType, IEntityDefFactory entityDefFactory, EntityDef sourceEntityDef, EntityDef targetEntityDef1, EntityDef targetEntityDef2)
            where TSource : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
            where TTarget3 : DatabaseEntity, new()
        {
            Func<IEntityDefFactory, IDataReader, object?> sourceFunc = GetCachedToEntityFunc(reader, sourceEntityDef, 0, sourceEntityDef.FieldCount, false, engineType);
            Func<IEntityDefFactory, IDataReader, object?> targetFunc1 = GetCachedToEntityFunc(reader, targetEntityDef1, sourceEntityDef.FieldCount, targetEntityDef1.FieldCount, true, engineType);
            Func<IEntityDefFactory, IDataReader, object?> targetFunc2 = GetCachedToEntityFunc(reader, targetEntityDef2, sourceEntityDef.FieldCount + targetEntityDef1.FieldCount, reader.FieldCount - (sourceEntityDef.FieldCount + targetEntityDef1.FieldCount), true, engineType);

            IList<Tuple<TSource, TTarget2?, TTarget3?>> lst = new List<Tuple<TSource, TTarget2?, TTarget3?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(entityDefFactory, reader)!;
                object? target1 = targetFunc1.Invoke(entityDefFactory, reader);
                object? target2 = targetFunc2.Invoke(entityDefFactory, reader);

                lst.Add(new Tuple<TSource, TTarget2?, TTarget3?>((TSource)source, (TTarget2?)target1, (TTarget3?)target2));
            }

            return lst;
        }

        private static Func<IEntityDefFactory, IDataReader, object?> GetCachedToEntityFunc(IDataReader reader, EntityDef entityDef, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType)
        {
            string key = GetKey(entityDef, startIndex, length, returnNullIfFirstNull, engineType);

            return _toEntityFuncDict.GetOrAdd(key, _ => EntityMapperDelegateCreator.CreateToEntityDelegate(entityDef, reader, startIndex, length, returnNullIfFirstNull, engineType));

            //if (!_toEntityFuncDict.ContainsKey(key))
            //{
            //    lock (_toEntityFuncDictLocker)
            //    {
            //        if (!_toEntityFuncDict.ContainsKey(key))
            //        {
            //            _toEntityFuncDict[key] = EntityMapperDelegateCreator.CreateToEntityDelegate(entityDef, reader, startIndex, length, returnNullIfFirstNull, engineType);
            //        }
            //    }
            //}

            //return _toEntityFuncDict[key];

            static string GetKey(EntityDef entityDef, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType)
            {
                return $"{engineType}_{entityDef.DatabaseName}_{entityDef.TableName}_{startIndex}_{length}_{returnNullIfFirstNull}";
            }
        }

        #endregion

        #region ToParameters(ToDb)

        private static readonly ConcurrentDictionary<string, Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]>> _toParametersFuncDict = new ConcurrentDictionary<string, Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]>>();

        //private static readonly object _toParameterFuncDictLocker = new object();

        public static IList<KeyValuePair<string, object>> ToParametersUsingReflection<T>(this T entity, EntityDef entityDef, EngineType engineType, int number = 0) where T : DatabaseEntity, new()
        {
            if (entity.Version < 0)
            {
                throw DatabaseExceptions.EntityVersionError(type: entityDef.EntityFullName, version: entity.Version, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(entityDef.FieldCount);

            foreach (var propertyDef in entityDef.PropertyDefs)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName!}_{number}",
                    TypeConvert.TypeValueToDbValue(propertyDef.GetValueFrom(entity), propertyDef, engineType)));
            }

            return parameters;
        }

        public static IList<KeyValuePair<string, object>> ToParameters(EntityDef entityDef, EngineType engineType, Dictionary<string, object?> propertyValues, int number = 0)
        {
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(propertyValues.Count);

            foreach (KeyValuePair<string, object?> kv in propertyValues)
            {
                EntityPropertyDef? propertyDef = entityDef.GetPropertyDef(kv.Key);

                if (propertyDef == null)
                {
                    throw DatabaseExceptions.PropertyNotFound(entityDef.EntityFullName, kv.Key);
                }

                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName}_{number}",
                    TypeConvert.TypeValueToDbValue(kv.Value, propertyDef, engineType)));
            }

            return parameters;
        }

        /// <summary>
        /// ToParameters. number为属性名的后缀数字
        /// </summary>
        public static IList<KeyValuePair<string, object>> ToParameters<T>(this T entity, EntityDef entityDef, EngineType engineType, IEntityDefFactory entityDefFactory, int number = 0) where T : DatabaseEntity, new()
        {
            if (entity.Version < 0)
            {
                throw DatabaseExceptions.EntityVersionError(type: entityDef.EntityFullName, version: entity.Version, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]> func = GetCachedToParametersFunc(entityDef, engineType);

            return func(entityDefFactory, entity, number);
        }

        private static Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]> GetCachedToParametersFunc(EntityDef entityDef, EngineType engineType)
        {
            string key = GetKey(entityDef, engineType);

            return _toParametersFuncDict.GetOrAdd(key, _ => EntityMapperDelegateCreator.CreateToParametersDelegate(entityDef, engineType));

            //if (!_toParametersFuncDict.ContainsKey(key))
            //{
            //    lock (_toParameterFuncDictLocker)
            //    {
            //        if (!_toParametersFuncDict.ContainsKey(key))
            //        {
            //            _toParametersFuncDict[key] = EntityMapperDelegateCreator.CreateToParametersDelegate(entityDef, engineType);
            //        }
            //    }
            //}

            //return _toParametersFuncDict[key];

            static string GetKey(EntityDef entityDef, EngineType engineType)
            {
                return $"{engineType}_{entityDef.DatabaseName}_{entityDef.TableName}_ToParameters";
            }
        }

        #endregion
    }
}