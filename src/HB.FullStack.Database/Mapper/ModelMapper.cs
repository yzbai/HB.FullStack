

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.DatabaseModels;
using HB.FullStack.Database.Engine;
using System.Linq;

namespace HB.FullStack.Database.Mapper
{
    internal static class ModelMapper
    {
        #region ToModel

        private static readonly ConcurrentDictionary<string, Func<IDatabaseModelDefFactory, IDataReader, object?>> _toModelFuncDict = new ConcurrentDictionary<string, Func<IDatabaseModelDefFactory, IDataReader, object?>>();

        public static IList<T> ToModels<T>(this IDataReader reader, EngineType engineType, IDatabaseModelDefFactory modelDefFactory, DatabaseModelDef modelDef)
            where T : DatabaseModel, new()
        {
            Func<IDatabaseModelDefFactory, IDataReader, object?> mapFunc = GetCachedToModelFunc(reader, modelDef, 0, reader.FieldCount, false, engineType);

            List<T> lst = new List<T>();

            while (reader.Read())
            {
                object item = mapFunc.Invoke(modelDefFactory, reader)!;

                lst.Add((T)item);
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget?>> ToModels<TSource, TTarget>(this IDataReader reader, EngineType engineType, IDatabaseModelDefFactory modelDefFactory, DatabaseModelDef sourceModelDef, DatabaseModelDef targetModelDef)
            where TSource : DatabaseModel, new()
            where TTarget : DatabaseModel, new()
        {
            Func<IDatabaseModelDefFactory, IDataReader, object?> sourceFunc = GetCachedToModelFunc(reader, sourceModelDef, 0, sourceModelDef.FieldCount, false, engineType);
            Func<IDatabaseModelDefFactory, IDataReader, object?> targetFunc = GetCachedToModelFunc(reader, targetModelDef, sourceModelDef.FieldCount, reader.FieldCount - sourceModelDef.FieldCount, true, engineType);

            IList<Tuple<TSource, TTarget?>> lst = new List<Tuple<TSource, TTarget?>>();

            while (reader.Read())
            {
                object source = sourceFunc.Invoke(modelDefFactory, reader)!;
                object? target = targetFunc.Invoke(modelDefFactory, reader);

                lst.Add(new Tuple<TSource, TTarget?>((TSource)source, (TTarget?)target));
            }

            return lst;
        }

        public static IList<Tuple<TSource, TTarget2?, TTarget3?>> ToModels<TSource, TTarget2, TTarget3>(this IDataReader reader, EngineType engineType, IDatabaseModelDefFactory modelDefFactory, DatabaseModelDef sourceModelDef, DatabaseModelDef targetModelDef1, DatabaseModelDef targetModelDef2)
            where TSource : DatabaseModel, new()
            where TTarget2 : DatabaseModel, new()
            where TTarget3 : DatabaseModel, new()
        {
            Func<IDatabaseModelDefFactory, IDataReader, object?> sourceFunc = GetCachedToModelFunc(reader, sourceModelDef, 0, sourceModelDef.FieldCount, false, engineType);
            Func<IDatabaseModelDefFactory, IDataReader, object?> targetFunc1 = GetCachedToModelFunc(reader, targetModelDef1, sourceModelDef.FieldCount, targetModelDef1.FieldCount, true, engineType);
            Func<IDatabaseModelDefFactory, IDataReader, object?> targetFunc2 = GetCachedToModelFunc(reader, targetModelDef2, sourceModelDef.FieldCount + targetModelDef1.FieldCount, reader.FieldCount - (sourceModelDef.FieldCount + targetModelDef1.FieldCount), true, engineType);

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

        private static Func<IDatabaseModelDefFactory, IDataReader, object?> GetCachedToModelFunc(IDataReader reader, DatabaseModelDef modelDef, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType)
        {
            string key = GetKey(modelDef, startIndex, length, returnNullIfFirstNull, engineType);

            return _toModelFuncDict.GetOrAdd(key, _ => ModelMapperDelegateCreator.CreateToModelDelegate(modelDef, reader, startIndex, length, returnNullIfFirstNull, engineType));

            //if (!_toModelFuncDict.ContainsKey(key))
            //{
            //    lock (_toModelFuncDictLocker)
            //    {
            //        if (!_toModelFuncDict.ContainsKey(key))
            //        {
            //            _toModelFuncDict[key] = ModelMapperDelegateCreator.CreateToModelDelegate(modelDef, reader, startIndex, length, returnNullIfFirstNull, engineType);
            //        }
            //    }
            //}

            //return _toModelFuncDict[key];

            static string GetKey(DatabaseModelDef modelDef, int startIndex, int length, bool returnNullIfFirstNull, EngineType engineType)
            {
                return $"{engineType}_{modelDef.DatabaseName}_{modelDef.TableName}_{startIndex}_{length}_{returnNullIfFirstNull}";
            }
        }

        #endregion

        #region ModelToParameters(ToDb)

        public static IList<KeyValuePair<string, object>> ModelToParametersUsingReflection<T>(this T model, DatabaseModelDef modelDef, EngineType engineType, int number = 0) where T : DatabaseModel, new()
        {
            if (model is ServerDatabaseModel serverModel && serverModel.Version < 0)
            {
                throw DatabaseExceptions.ModelVersionError(type: modelDef.ModelFullName, version: serverModel.Version, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(modelDef.FieldCount);

            foreach (var propertyDef in modelDef.PropertyDefs)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName!}_{number}",
                    TypeConvert.TypeValueToDbValue(propertyDef.GetValueFrom(model), propertyDef, engineType)));
            }

            return parameters;
        }

        private static readonly ConcurrentDictionary<string, Func<IDatabaseModelDefFactory, object, int, KeyValuePair<string, object>[]>> _entiryToParametersFuncDict =
            new ConcurrentDictionary<string, Func<IDatabaseModelDefFactory, object, int, KeyValuePair<string, object>[]>>();

        /// <summary>
        /// ModelToParameters. number为属性名的后缀数字
        /// </summary>
        public static IList<KeyValuePair<string, object>> ModelToParameters<T>(this T model, DatabaseModelDef modelDef, EngineType engineType, IDatabaseModelDefFactory modelDefFactory, int number = 0) where T : DatabaseModel, new()
        {
            if (model is ServerDatabaseModel serverModel && serverModel.Version < 0)
            {
                throw DatabaseExceptions.ModelVersionError(type: modelDef.ModelFullName, version: serverModel.Version, cause: "DatabaseVersionNotSet, 查看是否是使用了Select + New这个组合");
            }

            Func<IDatabaseModelDefFactory, object, int, KeyValuePair<string, object>[]> func = GetCachedModelToParametersFunc(modelDef, engineType);

            return func(modelDefFactory, model, number);
        }

        private static Func<IDatabaseModelDefFactory, object, int, KeyValuePair<string, object>[]> GetCachedModelToParametersFunc(DatabaseModelDef modelDef, EngineType engineType)
        {
            string key = GetKey(modelDef, engineType);

            return _entiryToParametersFuncDict.GetOrAdd(key, _ => ModelMapperDelegateCreator.CreateModelToParametersDelegate(modelDef, engineType));

            static string GetKey(DatabaseModelDef modelDef, EngineType engineType)
            {
                return $"{engineType}_{modelDef.DatabaseName}_{modelDef.TableName}_ModelToParameters";
            }
        }

        #endregion

        #region PropertyValuesToParameters

        public static IList<KeyValuePair<string, object>> PropertyValuesToParametersUsingReflection(
            DatabaseModelDef modelDef, EngineType engineType, IDictionary<string, object?> propertyValues, string parameterNameSuffix = "0")
        {
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(propertyValues.Count);

            foreach (KeyValuePair<string, object?> kv in propertyValues)
            {
                DatabaseModelPropertyDef? propertyDef = modelDef.GetPropertyDef(kv.Key);

                if (propertyDef == null)
                {
                    throw DatabaseExceptions.PropertyNotFound(modelDef.ModelFullName, kv.Key);
                }

                parameters.Add(new KeyValuePair<string, object>(
                    $"{propertyDef.DbParameterizedName}_{parameterNameSuffix}",
                    TypeConvert.TypeValueToDbValue(kv.Value, propertyDef, engineType)));
            }

            return parameters;
        }

        private static readonly ConcurrentDictionary<string, Func<IDatabaseModelDefFactory, object?[], string, KeyValuePair<string, object>[]>> _propertyValuesToParametersFuncDict =
            new ConcurrentDictionary<string, Func<IDatabaseModelDefFactory, object?[], string, KeyValuePair<string, object>[]>>();

        private static Func<IDatabaseModelDefFactory, object?[], string, KeyValuePair<string, object>[]> GetCachedPropertyValuesToParametersFunc(
            DatabaseModelDef modelDef, EngineType engineType, IList<string> propertyNames)
        {
            string key = GetKey(modelDef, engineType, propertyNames);

            return _propertyValuesToParametersFuncDict.GetOrAdd(key, _ => ModelMapperDelegateCreator.CreatePropertyValuesToParametersDelegate(modelDef, engineType, propertyNames));

            static string GetKey(DatabaseModelDef modelDef, EngineType engineType, IList<string> names)
            {
                return $"{engineType}_{modelDef.DatabaseName}_{modelDef.TableName}_{SecurityUtil.GetHash(names)}_PropertyValuesToParameters";
            }
        }

        public static KeyValuePair<string, object>[] PropertyValuesToParameters(
            DatabaseModelDef modelDef, EngineType engineType, IDatabaseModelDefFactory modelDefFactory, IList<string> propertyNames, IList<object?> propertyValues, string parameterNameSuffix = "0")
        {
            Func<IDatabaseModelDefFactory, object?[], string, KeyValuePair<string, object>[]> func = GetCachedPropertyValuesToParametersFunc(modelDef, engineType, propertyNames);

            return func(modelDefFactory, propertyValues.ToArray(), parameterNameSuffix);
        }

        #endregion
    }
}