﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

using HB.FullStack.Database.Engine;

using Microsoft;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder
    {
        public DbEngineCommand CreateDeleteIgnoreConflictCheckCommand(DbModelDef modelDef, object id, string lastUser, bool trulyDelete, long? newTimestamp = null)
        {
            List<string> propertyNames = new List<string> { nameof(DbModel2<long>.Id), nameof(BaseDbModel.LastUser) };
            List<object?> propertyValues = new List<object?> { id, lastUser };

            if (modelDef.IsTimestamp)
            {
                propertyNames.Add(nameof(ITimestamp.Timestamp));
                propertyValues.Add(newTimestamp ?? TimeUtil.Timestamp);
            }

            var parameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                propertyNames,
                propertyValues);

            return new DbEngineCommand(
                SqlHelper.CreateDeleteIgnoreConflictCheckSql(modelDef, trulyDelete),
                parameters);
        }

        public DbEngineCommand CreateDeleteTimestampCommand(DbModelDef modelDef, object id, long timestamp, string lastUser, bool trulyDelete, long? newTimestamp = null)
        {
            var parameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                new List<string> { nameof(DbModel2<long>.Id), nameof(BaseDbModel.LastUser), nameof(ITimestamp.Timestamp) },
                new List<object?> { id, lastUser, timestamp });

            parameters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.NEW_PROPERTY_VALUE_SUFFIX}_0", newTimestamp ?? TimeUtil.Timestamp));

            return new DbEngineCommand(
                SqlHelper.CreateDeleteUsingTimestampSql(modelDef, trulyDelete),
                parameters);
        }

        public DbEngineCommand CreateDeleteOldNewCompareCommand<T>(DbModelDef modelDef, T model, string lastUser, bool trulyDelete, long? newTimestamp = null) where T : BaseDbModel, new()
        {
            var parameters = model.ToDbParameters(modelDef, _modelDefFactory);

            var newParameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                new List<string> { nameof(BaseDbModel.LastUser) },
                new List<object?> { lastUser });

            parameters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.NEW_PROPERTY_VALUE_SUFFIX}_0", newTimestamp ?? TimeUtil.Timestamp));

            return new DbEngineCommand(
                SqlHelper.CreateDeleteUsingOldNewCompareSql(modelDef, trulyDelete),
                parameters.AddRange(newParameters));
        }

        public DbEngineCommand CreateDeleteCommand<T>(
            DbModelDef modelDef,
            WhereExpression<T> whereExpression,
            string lastUser,
            bool trulyDeleted) where T : TimelessDbModel, new()
        {
            Requires.NotNull(whereExpression, nameof(whereExpression));

            IList<KeyValuePair<string, object>> parameters = whereExpression.GetParameters();

            if (!trulyDeleted)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{SqlHelper.DbParameterName_LastUser}_0",
                    lastUser));
                parameters.Add(new KeyValuePair<string, object>(
                    $"{SqlHelper.DbParameterName_Deleted}_0",
                    true));

                string sql = GetCachedSql(SqlType.UpdateDeletedFields, new DbModelDef[] { modelDef }) + whereExpression.ToStatement();

                return new DbEngineCommand(sql, parameters);
            }

            string deleteSql = GetCachedSql(SqlType.Delete, new DbModelDef[] { modelDef }) + whereExpression.ToStatement();

            return new DbEngineCommand(deleteSql, parameters);
        }


        public DbEngineCommand CreateBatchDeleteIgnoreConflictCheckCommand(DbModelDef modelDef, IList<object> ids, string lastUser, bool trulyDeleted, long? newTimestamp = null)
        {
            ThrowIf.NullOrEmpty(ids, nameof(ids));

            IList<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();

            List<string> propertyNames = new List<string> { nameof(DbModel2<long>.Id), nameof(BaseDbModel.LastUser) };

            if (modelDef.IsTimestamp)
            {
                propertyNames.Add(nameof(ITimestamp.Timestamp));
            }

            long curTimestamp = TimeUtil.Timestamp;

            foreach (var id in ids)
            {
                List<object?> propertyValues = new List<object?> { id, lastUser };

                if (modelDef.IsTimestamp)
                {
                    propertyValues.Add(newTimestamp ?? curTimestamp);
                }

                var parameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    propertyNames,
                    propertyValues);

                totalParameters.AddRange(parameters);
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchDeleteIgnoreConflictCheckSql(modelDef, trulyDeleted, ids.Count),
                totalParameters);
        }

        public DbEngineCommand CreateBatchDeleteTimestampCommand(DbModelDef modelDef, IList<object> ids, IList<long> timestamps, string lastUser, bool trulyDelete, long? newTimestamp = null)
        {
            IList<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();

            return new DbEngineCommand(
                SqlHelper.CreateBatchDeleteUsingTimestampSql(modelDef, trulyDelete, ids.Count), 
                totalParameters);
        }

        public DbEngineCommand CreateBatchDeleteCommand(
            DbModelDef modelDef,
            IList<object> ids,
            IList<long?> oldTimestamps,
            IList<long?> newTimestamps,
            string lastUser,
            bool trulyDeleted,
            bool needTrans)
        {
            int count = ids.Count;

            DbEngineType engineType = modelDef.EngineType;

            if (!trulyDeleted)
            {
                List<string> propertyNames = new List<string> { nameof(DbModel.Deleted) };
                List<object?> propertyValues = new List<object?> { true };
                List<TimestampUpdatePack> updatePacks = new List<TimestampUpdatePack>();

                for (int i = 0; i < count; ++i)
                {
                    updatePacks.Add(new TimestampUpdatePack
                    {
                        Id = ids[i],
                        OldTimestamp = oldTimestamps[i],
                        NewTimestamp = newTimestamps[i],
                        PropertyNames = propertyNames,
                        NewPropertyValues = propertyValues
                    });
                }

                return CreateBatchUpdatePropertiesTimestampCommand(modelDef, updatePacks, lastUser, needTrans);
            }

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            for (int i = 0; i < count; ++i)
            {
                List<string> propertyNames = new List<string> { nameof(TimestampLongIdDbModel.Id) };
                List<object?> propertyValues = new List<object?> { ids[i] };

                if (modelDef.IsTimestamp && !oldTimestamps[i].HasValue)
                {
                    throw DbExceptions.TimestampNotExists(engineType, modelDef, propertyNames);
                }

                if (oldTimestamps[i].HasValue)
                {
                    propertyNames.Add(nameof(TimestampLongIdDbModel.Timestamp));
                    propertyValues.Add(oldTimestamps[i]!.Value);
                }

                IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(modelDef, _modelDefFactory, propertyNames, propertyValues, number.ToString());

                totalParameters.AddRange(parameters);

                string sql = SqlHelper.CreateDeleteByPropertiesSql(modelDef, propertyNames, number);

#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{sql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundDeletedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{sql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundDeletedRows_Statement(engineType), engineType)}");
#endif

                number++;
            }

            string may_trans_begin = needTrans ? SqlHelper.Transaction_Begin(engineType) : "";
            string may_trans_commit = needTrans ? SqlHelper.Transaction_Commit(engineType) : "";

            string commandText = $@"{may_trans_begin}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Create_Id(tempTableName, engineType)}
                                    {innerBuilder}
                                    {SqlHelper.TempTable_Select_Id(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {may_trans_commit}";

            return new DbEngineCommand(commandText, totalParameters);
        }
    }
}
