﻿
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Convert
{
    /// <summary>
    /// 整体转换，某一个值得转换参看DbValueConverter
    /// </summary>
    public static partial class DbModelConvert
    {
        #region IDataReader Row To Model

        public static IList<T> ToDbModels<T>(this IDataReader reader, IDbModelDefFactory modelDefFactory, DbModelDef modelDef)
            where T : DbModel, new()
        {
            Func<IDbModelDefFactory, IDataReader, object?> mapFunc = GetCachedDataReaderRowToModelFunc(reader, modelDef, 0, reader.FieldCount, false);

            List<T> lst = new List<T>();

            while (reader.Read())
            {
                object item = mapFunc.Invoke(modelDefFactory, reader)!;

                lst.Add((T)item);
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget?>> ToDbModels<TSource, TTarget>(this IDataReader reader, IDbModelDefFactory modelDefFactory, DbModelDef sourceModelDef, DbModelDef targetModelDef)
            where TSource : DbModel, new()
            where TTarget : DbModel, new()
        {
            Func<IDbModelDefFactory, IDataReader, object?> sourceFunc = GetCachedDataReaderRowToModelFunc(reader, sourceModelDef, 0, sourceModelDef.FieldCount, false);
            Func<IDbModelDefFactory, IDataReader, object?> targetFunc = GetCachedDataReaderRowToModelFunc(reader, targetModelDef, sourceModelDef.FieldCount, reader.FieldCount - sourceModelDef.FieldCount, true);

            IList<Tuple<TSource, TTarget?>> lst = new List<Tuple<TSource, TTarget?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(modelDefFactory, reader)!;
                object? target = targetFunc.Invoke(modelDefFactory, reader);

                lst.Add(new Tuple<TSource, TTarget?>((TSource)source, (TTarget?)target));
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget2?, TTarget3?>> ToDbModels<TSource, TTarget2, TTarget3>(this IDataReader reader, IDbModelDefFactory modelDefFactory, DbModelDef sourceModelDef, DbModelDef targetModelDef1, DbModelDef targetModelDef2)
            where TSource : DbModel, new()
            where TTarget2 : DbModel, new()
            where TTarget3 : DbModel, new()
        {
            Func<IDbModelDefFactory, IDataReader, object?> sourceFunc = GetCachedDataReaderRowToModelFunc(reader, sourceModelDef, 0, sourceModelDef.FieldCount, false);
            Func<IDbModelDefFactory, IDataReader, object?> targetFunc1 = GetCachedDataReaderRowToModelFunc(reader, targetModelDef1, sourceModelDef.FieldCount, targetModelDef1.FieldCount, true);
            Func<IDbModelDefFactory, IDataReader, object?> targetFunc2 = GetCachedDataReaderRowToModelFunc(reader, targetModelDef2, sourceModelDef.FieldCount + targetModelDef1.FieldCount, reader.FieldCount - (sourceModelDef.FieldCount + targetModelDef1.FieldCount), true);

            IList<Tuple<TSource, TTarget2?, TTarget3?>> lst = new List<Tuple<TSource, TTarget2?, TTarget3?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(modelDefFactory, reader)!;
                object? target1 = targetFunc1.Invoke(modelDefFactory, reader);
                object? target2 = targetFunc2.Invoke(modelDefFactory, reader);

                lst.Add(new Tuple<TSource, TTarget2?, TTarget3?>((TSource)source, (TTarget2?)target1, (TTarget3?)target2));
            }

            return lst;
        }

        private static readonly ConcurrentDictionary<string, Func<IDbModelDefFactory, IDataReader, object?>> _toDbModelFuncDict = new ConcurrentDictionary<string, Func<IDbModelDefFactory, IDataReader, object?>>();

        private static Func<IDbModelDefFactory, IDataReader, object?> GetCachedDataReaderRowToModelFunc(IDataReader reader, DbModelDef modelDef, int startIndex, int length, bool returnNullIfFirstNull)
        {
            string key = GetKey(modelDef, startIndex, length, returnNullIfFirstNull);

            return _toDbModelFuncDict.GetOrAdd(key, _ => CreateDataReaderRowToModelDelegate(modelDef, reader, startIndex, length, returnNullIfFirstNull));

            //if (!_toDbModelFuncDict.ContainsKey(key))
            //{
            //    lock (_toModelFuncDictLocker)
            //    {
            //        if (!_toDbModelFuncDict.ContainsKey(key))
            //        {
            //            _toDbModelFuncDict[key] = DBModelConverterEmit.CreateDataReaderRowToModelDelegate(modelDef, reader, startIndex, length, returnNullIfFirstNull, engineType);
            //        }
            //    }
            //}

            //return _toDbModelFuncDict[key];

            static string GetKey(DbModelDef modelDef, int startIndex, int length, bool returnNullIfFirstNull)
            {
                return $"{modelDef.DbSchemaName}_{modelDef.TableName}_{startIndex}_{length}_{returnNullIfFirstNull}";
            }
        }

        #endregion

        #region Model To DbParameters

        public static IList<KeyValuePair<string, object>> ToDbParametersUsingReflection<T>(this T model, DbModelDef modelDef, int number = 0) where T : DbModel, new()
        {
            if (model is TimestampDbModel serverModel && serverModel.Timestamp <= 0)
            {
                throw DbExceptions.ModelTimestampError(type: modelDef.ModelFullName, timestamp: serverModel.Timestamp, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(modelDef.FieldCount);
            DbEngineType engineType = modelDef.EngineType;

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName!}_{number}",
                    DbPropertyConvert.PropertyValueToDbFieldValue(propertyDef.GetValueFrom(model), propertyDef, engineType)));
            }

            return parameters;
        }

        /// <summary>
        /// ToDbParameters. number为属性名的后缀数字
        /// </summary>
        public static IList<KeyValuePair<string, object>> ToDbParameters<T>(this T model, DbModelDef modelDef, IDbModelDefFactory modelDefFactory, int number = 0) where T : DbModel, new()
        {
            if (model is TimestampDbModel serverModel && serverModel.Timestamp <= 0)
            {
                throw DbExceptions.ModelTimestampError(type: modelDef.ModelFullName, timestamp: serverModel.Timestamp, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            Func<IDbModelDefFactory, object, int, KeyValuePair<string, object>[]> func = GetCachedModelToParametersFunc(modelDef);

            return new List<KeyValuePair<string, object>>(func(modelDefFactory, model, number));
        }

        private static readonly ConcurrentDictionary<string, Func<IDbModelDefFactory, object, int, KeyValuePair<string, object>[]>> _toDbParametersFuncDict = new ConcurrentDictionary<string, Func<IDbModelDefFactory, object, int, KeyValuePair<string, object>[]>>();

        private static Func<IDbModelDefFactory, object, int, KeyValuePair<string, object>[]> GetCachedModelToParametersFunc(DbModelDef modelDef)
        {
            string key = GetKey(modelDef);

            return _toDbParametersFuncDict.GetOrAdd(key, _ => CreateModelToDbParametersDelegate(modelDef));

            static string GetKey(DbModelDef modelDef)
            {
                return $"{modelDef.DbSchemaName}_{modelDef.TableName}_ModelToParameters";
            }
        }

        #endregion

        #region PropertyValues To DbParameters

        public static IList<KeyValuePair<string, object>> PropertyValuesToParametersUsingReflection(
            DbModelDef modelDef, IDictionary<string, object?> propertyValues, string parameterNameSuffix = "0")
        {
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(propertyValues.Count);

            DbEngineType engineType = modelDef.EngineType;

            foreach (KeyValuePair<string, object?> kv in propertyValues)
            {
                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(kv.Key);

                    if (propertyDef == null)
                {
                    throw DbExceptions.PropertyNotFound(modelDef.ModelFullName, kv.Key);
                }

                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName}_{parameterNameSuffix}",
                    DbPropertyConvert.PropertyValueToDbFieldValue(kv.Value, propertyDef, engineType)));
            }

            return parameters;
        }

        private static readonly ConcurrentDictionary<string, Func<IDbModelDefFactory, object?[], string, KeyValuePair<string, object>[]>> _propertyValuesToParametersFuncDict =
            new ConcurrentDictionary<string, Func<IDbModelDefFactory, object?[], string, KeyValuePair<string, object>[]>>();

        private static Func<IDbModelDefFactory, object?[], string, KeyValuePair<string, object>[]> GetCachedPropertyValuesToParametersFunc(
            DbModelDef modelDef, IList<string> propertyNames)
        {
            string key = GetKey(modelDef, propertyNames);

            return _propertyValuesToParametersFuncDict.GetOrAdd(key, _ => CreatePropertyValuesToParametersDelegate(modelDef, propertyNames));

            static string GetKey(DbModelDef modelDef, IList<string> names)
            {
                return $"{modelDef.DbSchemaName}_{modelDef.TableName}_{SecurityUtil.GetHash(names)}_PropertyValuesToParameters";
            }
        }

        public static IList<KeyValuePair<string, object>> PropertyValuesToParameters(
            DbModelDef modelDef, IDbModelDefFactory modelDefFactory, IList<string> propertyNames, IList<object?> propertyValues, string parameterNameSuffix = "0")
        {
            Func<IDbModelDefFactory, object?[], string, KeyValuePair<string, object>[]> func = GetCachedPropertyValuesToParametersFunc(modelDef, propertyNames);

            return new List<KeyValuePair<string, object>>(func(modelDefFactory, propertyValues.ToArray(), parameterNameSuffix));
        }

        #endregion
    }
}