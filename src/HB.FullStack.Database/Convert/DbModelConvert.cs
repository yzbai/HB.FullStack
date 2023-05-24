
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using HB.FullStack.Common;
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
            where T : IDbModel
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
            where TSource : IDbModel
            where TTarget : IDbModel
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
            where TSource : IDbModel
            where TTarget2 : IDbModel
            where TTarget3 : IDbModel
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
                return $"{modelDef.DbSchema.Name}_{modelDef.TableName}_{startIndex}_{length}_{returnNullIfFirstNull}";
            }
        }

        #endregion

        #region Model To DbParameters

        public static IList<KeyValuePair<string, object>> ToDbParametersUsingReflection<T>(this T model, DbModelDef modelDef, string? parameterNameSuffix, int number) where T : IDbModel
        {
            if (model is ITimestamp serverModel && serverModel.Timestamp <= 0)
            {
                throw DbExceptions.ModelTimestampError(type: modelDef.FullName, timestamp: serverModel.Timestamp, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(modelDef.FieldCount);
            DbEngineType engineType = modelDef.EngineType;

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName!}_{parameterNameSuffix}{number}",
                    DbPropertyConvert.PropertyValueToDbFieldValue(propertyDef.GetValueFrom(model), propertyDef, engineType)));
            }

            return parameters;
        }

        public static IList<KeyValuePair<string, object>> ToDbParameters<T>(this IEnumerable<T> models, DbModelDef modelDef, IDbModelDefFactory modelDefFactory, string? parameterNameSuffix) where T : IDbModel
        {
            int number = 0;
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

            foreach (T model in models)
            {
                parameters.AddRange(model.ToDbParameters(modelDef, modelDefFactory, parameterNameSuffix, number));

                ++number;
            }

            return parameters;
        }

        public static IList<KeyValuePair<string, object>> ToDbParameters<T>(this T model, DbModelDef modelDef, IDbModelDefFactory modelDefFactory, string? parameterNameSuffix, int number) where T : IDbModel
        {
            if (model is ITimestamp serverModel && serverModel.Timestamp <= 0)
            {
                throw DbExceptions.ModelTimestampError(type: modelDef.FullName, timestamp: serverModel.Timestamp, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            Func<IDbModelDefFactory, object, string, KeyValuePair<string, object>[]> func = GetCachedModelToParametersFunc(modelDef);

            return new List<KeyValuePair<string, object>>(func(modelDefFactory, model, $"{parameterNameSuffix}{number}"));
        }

        private static readonly ConcurrentDictionary<string, Func<IDbModelDefFactory, object, string, KeyValuePair<string, object>[]>> _toDbParametersFuncDict = new ConcurrentDictionary<string, Func<IDbModelDefFactory, object, string, KeyValuePair<string, object>[]>>();

        private static Func<IDbModelDefFactory, object, string, KeyValuePair<string, object>[]> GetCachedModelToParametersFunc(DbModelDef modelDef)
        {
            string key = GetKey(modelDef);

            return _toDbParametersFuncDict.GetOrAdd(key, _ => CreateModelToDbParametersDelegate(modelDef));

            static string GetKey(DbModelDef modelDef)
            {
                return $"{modelDef.DbSchema.Name}_{modelDef.TableName}_ModelToParameters";
            }
        }

        #endregion

        #region PropertyValues To DbParameters

        public static IList<KeyValuePair<string, object>> AddParameter(this IList<KeyValuePair<string, object>> parameters,
            DbModelPropertyDef propertyDef, object propertyValue, string? parameterSuffix, int number)
        {

            parameters.Add(new KeyValuePair<string, object>(
                $"{propertyDef.DbParameterizedName}_{parameterSuffix}{number}",
                DbPropertyConvert.PropertyValueToDbFieldValue(propertyValue, propertyDef, propertyDef.ModelDef.EngineType)));

            return parameters;
        }

        public static IList<KeyValuePair<string, object>> PropertyValuesToParametersUsingReflection(
            DbModelDef modelDef, IDictionary<string, object?> propertyValues, string? parameterNameSuffix, int number)
        {
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(propertyValues.Count);

            DbEngineType engineType = modelDef.EngineType;

            foreach (KeyValuePair<string, object?> kv in propertyValues)
            {
                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(kv.Key);

                if (propertyDef == null)
                {
                    throw DbExceptions.PropertyNotFound(modelDef.FullName, kv.Key);
                }

                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName}_{parameterNameSuffix}{number}",
                    DbPropertyConvert.PropertyValueToDbFieldValue(kv.Value, propertyDef, engineType)));
            }

            return parameters;
        }

        public static IList<KeyValuePair<string, object>> PropertyValuesToParameters(
                    DbModelDef modelDef, IDbModelDefFactory modelDefFactory, IList<string> propertyNames, IList<object?> propertyValues, string? parameterNameSuffix, int number)
        {
            Func<IDbModelDefFactory, object?[], string, KeyValuePair<string, object>[]> func = GetCachedPropertyValuesToParametersFunc(modelDef, propertyNames);

            return new List<KeyValuePair<string, object>>(func(modelDefFactory, propertyValues.ToArray(), $"{parameterNameSuffix}{number}"));
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
                return $"{modelDef.DbSchema.Name}_{modelDef.TableName}_{SecurityUtil.GetHash(names)}_PropertyValuesToParameters";
            }
        }

        #endregion
    }
}