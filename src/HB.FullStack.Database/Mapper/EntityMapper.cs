

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Engine;
using System.Linq;

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

        #region EntityToParameters(ToDb)

        public static IList<KeyValuePair<string, object>> EntityToParametersUsingReflection<T>(this T entity, EntityDef entityDef, EngineType engineType, int number = 0) where T : DatabaseEntity, new()
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

        private static readonly ConcurrentDictionary<string, Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]>> _entiryToParametersFuncDict =
            new ConcurrentDictionary<string, Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]>>();

        /// <summary>
        /// EntityToParameters. number为属性名的后缀数字
        /// </summary>
        public static IList<KeyValuePair<string, object>> EntityToParameters<T>(this T entity, EntityDef entityDef, EngineType engineType, IEntityDefFactory entityDefFactory, int number = 0) where T : DatabaseEntity, new()
        {
            if (entity.Version < 0)
            {
                throw DatabaseExceptions.EntityVersionError(type: entityDef.EntityFullName, version: entity.Version, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]> func = GetCachedEntityToParametersFunc(entityDef, engineType);

            return func(entityDefFactory, entity, number);
        }

        private static Func<IEntityDefFactory, object, int, KeyValuePair<string, object>[]> GetCachedEntityToParametersFunc(EntityDef entityDef, EngineType engineType)
        {
            string key = GetKey(entityDef, engineType);

            return _entiryToParametersFuncDict.GetOrAdd(key, _ => EntityMapperDelegateCreator.CreateEntityToParametersDelegate(entityDef, engineType));

            static string GetKey(EntityDef entityDef, EngineType engineType)
            {
                return $"{engineType}_{entityDef.DatabaseName}_{entityDef.TableName}_EntityToParameters";
            }
        }

        #endregion

        #region PropertyValuesToParameters

        public static IList<KeyValuePair<string, object>> PropertyValuesToParametersUsingReflection(
            EntityDef entityDef, EngineType engineType, IDictionary<string, object?> propertyValues, string parameterNameSuffix = "0")
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
                    $"{propertyDef.DbParameterizedName}_{parameterNameSuffix}",
                    TypeConvert.TypeValueToDbValue(kv.Value, propertyDef, engineType)));
            }

            return parameters;
        }

        private static readonly ConcurrentDictionary<string, Func<IEntityDefFactory, object?[], string, KeyValuePair<string, object>[]>> _propertyValuesToParametersFuncDict =
            new ConcurrentDictionary<string, Func<IEntityDefFactory, object?[], string, KeyValuePair<string, object>[]>>();

        private static Func<IEntityDefFactory, object?[], string, KeyValuePair<string, object>[]> GetCachedPropertyValuesToParametersFunc(
            EntityDef entityDef, EngineType engineType, IList<string> propertyNames)
        {
            string key = GetKey(entityDef, engineType, propertyNames);

            return _propertyValuesToParametersFuncDict.GetOrAdd(key, _ => EntityMapperDelegateCreator.CreatePropertyValuesToParametersDelegate(entityDef, engineType, propertyNames));

            static string GetKey(EntityDef entityDef, EngineType engineType, IList<string> names)
            {
                return $"{engineType}_{entityDef.DatabaseName}_{entityDef.TableName}_{SecurityUtil.GetHash(names)}_PropertyValuesToParameters";
            }
        }

        public static KeyValuePair<string, object>[] PropertyValuesToParameters(
            EntityDef entityDef, EngineType engineType, IEntityDefFactory entityDefFactory, IList<string> propertyNames, IList<object?> propertyValues, string parameterNameSuffix = "0")
        {
            Func<IEntityDefFactory, object?[], string, KeyValuePair<string, object>[]> func = GetCachedPropertyValuesToParametersFunc(entityDef, engineType, propertyNames);

            return func(entityDefFactory, propertyValues.ToArray(), parameterNameSuffix);
        }

        #endregion
    }
}