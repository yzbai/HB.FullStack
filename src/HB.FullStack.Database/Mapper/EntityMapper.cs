#nullable enable

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

        private static readonly ConcurrentDictionary<string, Func<IDataReader, object?>> _toEntityFuncDict = new ConcurrentDictionary<string, Func<IDataReader, object?>>();

        private static readonly object _toEntityFuncDictLocker = new object();

        /// <summary>
        /// ToEntities
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="engineType"></param>
        /// <param name="entityDef"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static IList<T> ToEntities<T>(this IDataReader reader, EngineType engineType, EntityDef entityDef)
            where T : DatabaseEntity, new()
        {
            Func<IDataReader, object?> mapFunc = GetCachedToEntityFunc(reader, entityDef, 0, reader.FieldCount, false, engineType);

            List<T> lst = new List<T>();

            while (reader.Read())
            {
                object item = mapFunc.Invoke(reader)!;

                lst.Add((T)item);
            }

            return lst;
        }

        /// <summary>
        /// ToEntities
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="engineType"></param>
        /// <param name="sourceEntityDef"></param>
        /// <param name="targetEntityDef"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static IList<Tuple<TSource, TTarget?>> ToEntities<TSource, TTarget>(this IDataReader reader, EngineType engineType, EntityDef sourceEntityDef, EntityDef targetEntityDef)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            var sourceFunc = GetCachedToEntityFunc(reader, sourceEntityDef, 0, sourceEntityDef.FieldCount, false, engineType);
            var targetFunc = GetCachedToEntityFunc(reader, targetEntityDef, sourceEntityDef.FieldCount, reader.FieldCount - sourceEntityDef.FieldCount, true, engineType);

            IList<Tuple<TSource, TTarget?>> lst = new List<Tuple<TSource, TTarget?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(reader)!;
                object? target = targetFunc.Invoke(reader);

                lst.Add(new Tuple<TSource, TTarget?>((TSource)source, (TTarget?)target));
            }

            return lst;
        }

        /// <summary>
        /// ToEntities
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="engineType"></param>
        /// <param name="sourceEntityDef"></param>
        /// <param name="targetEntityDef1"></param>
        /// <param name="targetEntityDef2"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static IList<Tuple<TSource, TTarget2?, TTarget3?>> ToEntities<TSource, TTarget2, TTarget3>(this IDataReader reader, EngineType engineType, EntityDef sourceEntityDef, EntityDef targetEntityDef1, EntityDef targetEntityDef2)
            where TSource : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
            where TTarget3 : DatabaseEntity, new()
        {
            var sourceFunc = GetCachedToEntityFunc(reader, sourceEntityDef, 0, sourceEntityDef.FieldCount, false, engineType);
            var targetFunc1 = GetCachedToEntityFunc(reader, targetEntityDef1, sourceEntityDef.FieldCount, targetEntityDef1.FieldCount, true, engineType);
            var targetFunc2 = GetCachedToEntityFunc(reader, targetEntityDef2, sourceEntityDef.FieldCount + targetEntityDef1.FieldCount, reader.FieldCount - (sourceEntityDef.FieldCount + targetEntityDef1.FieldCount), true, engineType);

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

        /// <summary>
        /// GetCachedToEntityFunc
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entityDef"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="returnNullIfFirstNull"></param>
        /// <param name="engineType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static Func<IDataReader, object?> GetCachedToEntityFunc(IDataReader reader, EntityDef entityDef, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType)
        {
            string key = GetKey(entityDef, startIndex, length, returnNullIfFirstNull, engineType);

            if (!_toEntityFuncDict.ContainsKey(key))
            {
                lock (_toEntityFuncDictLocker)
                {
                    if (!_toEntityFuncDict.ContainsKey(key))
                    {
                        _toEntityFuncDict[key] = EntityMapperDelegateCreator.CreateToEntityDelegate(entityDef, reader, startIndex, length, returnNullIfFirstNull, engineType);
                    }
                }
            }

            return _toEntityFuncDict[key];

            static string GetKey(EntityDef entityDef, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType)
            {
                return $"{engineType}_{entityDef.DatabaseName}_{entityDef.TableName}_{startIndex}_{length}_{returnNullIfFirstNull}";
            }
        }

        #endregion

        #region ToParameters

        private static readonly ConcurrentDictionary<string, Func<object, int, KeyValuePair<string, object>[]>> _toParametersFuncDict = new ConcurrentDictionary<string, Func<object, int, KeyValuePair<string, object>[]>>();

        private static readonly object _toParameterFuncDictLocker = new object();

        /// <summary>
        /// ToParametersUsingReflection
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityDef"></param>
        /// <param name="engineType"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static IList<KeyValuePair<string, object>> ToParametersUsingReflection<T>(this T entity, EntityDef entityDef, EngineType engineType, int number = 0) where T : DatabaseEntity, new()
        {
            if (entity.Version < 0)
            {
                throw Exceptions.EntityVersionError(type:entityDef.EntityFullName,version: entity.Version, cause:"DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
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

        /// <summary>
        /// ToParameters
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityDef"></param>
        /// <param name="engineType"></param>
        /// <param name="number">属性名的后缀数字</param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static IList<KeyValuePair<string, object>> ToParameters<T>(this T entity, EntityDef entityDef, EngineType engineType, int number = 0) where T : DatabaseEntity, new()
        {
            if (entity.Version < 0)
            {
                throw Exceptions.EntityVersionError(type: entityDef.EntityFullName, version: entity.Version, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            Func<object, int, KeyValuePair<string, object>[]> func = GetCachedToParametersFunc(entityDef, engineType);

            return func(entity, number);
        }

        private static Func<object, int, KeyValuePair<string, object>[]> GetCachedToParametersFunc(EntityDef entityDef, EngineType engineType)
        {
            string key = GetKey(entityDef, engineType);

            if (!_toParametersFuncDict.ContainsKey(key))
            {
                lock (_toParameterFuncDictLocker)
                {
                    if (!_toParametersFuncDict.ContainsKey(key))
                    {
                        _toParametersFuncDict[key] = EntityMapperDelegateCreator.CreateToParametersDelegate(entityDef, engineType);
                    }
                }
            }

            return _toParametersFuncDict[key];

            static string GetKey(EntityDef entityDef, EngineType engineType)
            {
                return $"{engineType}_{entityDef.DatabaseName}_{entityDef.TableName}_ToParameters";
            }
        }

        #endregion
    }
}