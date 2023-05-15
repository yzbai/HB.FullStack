using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Meta;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder
    {
        public DbEngineCommand CreateUpdatePropertiesTimestampCommand(DbModelDef modelDef, TimestampUpdatePack updatePack, string lastUser)
        {
            modelDef.ThrowIfNotTimestamp();

            if (!updatePack.OldTimestamp.HasValue)
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} propertyNames: {updatePack.PropertyNames.ToJoinedString(",")}. Lack of OldTimestamp.");
            }

            IList<string> updatedPropertyNames = new List<string>(updatePack.PropertyNames)
            {
                nameof(DbModel2<long>.Id),
                nameof(BaseDbModel.LastUser),
                nameof(ITimestamp.Timestamp)
            };
            IList<object?> updatedPropertyValues = new List<object?>(updatePack.NewPropertyValues)
            {
                updatePack.Id,
                lastUser,
                updatePack.NewTimestamp ?? TimeUtil.Timestamp
            };

            IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                updatedPropertyNames,
                updatedPropertyValues);

            parameters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0", updatePack.OldTimestamp.Value));

            return new DbEngineCommand(
                GetCachedSql(SqlType.UpdatePropertiesUsingTimestamp, new DbModelDef[] { modelDef }, updatedPropertyNames),
                parameters);
        }

        public DbEngineCommand CreateBatchUpdatePropertiesTimestampCommand(DbModelDef modelDef, IList<TimestampUpdatePack> updatePacks, string lastUser)
        {
            modelDef.ThrowIfNotTimestamp();

            DbEngineType engineType = modelDef.EngineType;

            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;
            StringBuilder innerSqlBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            long curTimestamp = TimeUtil.Timestamp;

            foreach (TimestampUpdatePack updatePack in updatePacks)
            {
                if (!updatePack.OldTimestamp.HasValue)
                {
                    throw DbExceptions.ConflictCheckError($"{modelDef.FullName} propertyNames: {updatePack.PropertyNames.ToJoinedString(",")}. Lack of OldTimestamp.");
                }

                IList<string> updatedPropertyNames = new List<string>(updatePack.PropertyNames)
                {
                    nameof(DbModel2<long>.Id),
                    nameof(BaseDbModel.LastUser),
                    nameof(ITimestamp.Timestamp)
                };
                IList<object?> updatedPropertyValues = new List<object?>(updatePack.NewPropertyValues)
                {
                    updatePack.Id,
                    lastUser,
                    updatePack.NewTimestamp ?? curTimestamp
                };

                IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    updatedPropertyNames,
                    updatedPropertyValues,
                    number.ToString());

                parameters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}", updatePack.OldTimestamp.Value));

                totalParameters.AddRange(parameters);

                //sql
                //Remark: 由于packs中的pack可能是各种各样的，所以这里不能用模板，像Update那样
                string updatePropertiesSql = SqlHelper.CreateUpdatePropertiesUsingTimestampSql(modelDef, updatedPropertyNames, number);
                innerSqlBuilder.Append($"{updatePropertiesSql} {SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");


                number++;
            }

            string commandText = $@"{SqlHelper.Transaction_Begin(engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Create_Id(tempTableName, engineType)}
                                    {innerSqlBuilder}
                                    {SqlHelper.TempTable_Select_Id(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.Transaction_Commit(engineType)}";

            return new DbEngineCommand(commandText, totalParameters);
        }

        public DbEngineCommand CreateUpdatePropertiesOldNewCompareCommand(DbModelDef modelDef, OldNewCompareUpdatePack updatePack, string lastUser)
        {
            var oldParameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                new List<string>(updatePack.PropertyNames) { nameof(DbModel2<long>.Id) },
                new List<object?>(updatePack.OldPropertyValues) { updatePack.Id },
                $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0");

            var newParameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                new List<string>(updatePack.PropertyNames) { nameof(BaseDbModel.LastUser) },
                new List<object?>(updatePack.NewPropertyValues) { lastUser },
                $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_0");

            //使用propertyNames而不是curPropertyNames
            string sql = GetCachedSql(SqlType.UpdatePropertiesUsingOldNewCompare, new DbModelDef[] { modelDef }, updatePack.PropertyNames);

            return new DbEngineCommand(sql, oldParameters.AddRange(newParameters));
        }

        public DbEngineCommand CreateBatchUpdatePropertiesOldNewCompareCommand(DbModelDef modelDef, IList<OldNewCompareUpdatePack> updatePacks, string lastUser)
        {
            //TODO:如果packs中的PropertyNames都相同，可以进一步提升性能
            ThrowIf.Empty(updatePacks, nameof(updatePacks));

            DbEngineType engineType = modelDef.EngineType;

            int number = 0;
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            StringBuilder innerBuilder = new StringBuilder();

            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();

            foreach (OldNewCompareUpdatePack updatePack in updatePacks)
            {
                #region Parameters

                var oldParameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    new List<string>(updatePack.PropertyNames) { nameof(DbModel2<long>.Id) },
                    new List<object?>(updatePack.OldPropertyValues) { updatePack.Id },
                    $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}");

                var newParameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    new List<string>(updatePack.PropertyNames) { nameof(BaseDbModel.LastUser) },
                    new List<object?>(updatePack.NewPropertyValues) { lastUser },
                    $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_{number}");

                totalParameters.AddRange(oldParameters);
                totalParameters.AddRange(newParameters);

                #endregion

                string sql = SqlHelper.CreateUpdatePropertiesUsingOldNewCompareSql(modelDef, updatePack.PropertyNames, number);

                innerBuilder.Append($"{sql} {SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");

                number++;
            }

            string commandText = $@"{SqlHelper.Transaction_Begin(engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Create_Id(tempTableName, engineType)}
                                    {innerBuilder}
                                    {SqlHelper.TempTable_Select_Id(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.Transaction_Commit(engineType)}";

            return new DbEngineCommand(commandText, totalParameters);
        }

        public DbEngineCommand CreateUpdatePropertiesIgnoreConflictCheckCommand(DbModelDef modelDef, IgnoreConflictCheckUpdatePack updatePack, string lastUser)
        {
            IList<string> updatedPropertyNames = new List<string>(updatePack.PropertyNames)
            {
                nameof(DbModel2<long>.Id),
                nameof(BaseDbModel.LastUser)
            };

            IList<object?> updatedPropertyValues = new List<object?>(updatePack.NewPropertyValues)
            {
                updatePack.Id,
                lastUser
            };

            IList<KeyValuePair<string, object>> paramters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                updatedPropertyNames,
                updatedPropertyValues);

            return new DbEngineCommand(
                GetCachedSql(SqlType.UpdatePropertiesIgnoreConflictCheck, new DbModelDef[] { modelDef }, updatedPropertyNames),
                paramters);
        }

        public DbEngineCommand CreateBatchUpdatePropertiesIgnoreConflictCheckCommand(DbModelDef modelDef, IList<IgnoreConflictCheckUpdatePack> updatePacks, string lastUser)
        {
            DbEngineType engineType = modelDef.EngineType;

            int number = 0;
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            long curTimestamp = TimeUtil.Timestamp;

            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            StringBuilder innerSqlBuilder = new StringBuilder();

            foreach (IgnoreConflictCheckUpdatePack updatePack in updatePacks)
            {
                IList<string> updatedPropertyNames = new List<string>(updatePack.PropertyNames)
                {
                    nameof(DbModel2<long>.Id),
                    nameof(BaseDbModel.LastUser),
                };
                IList<object?> updatedPropertyValues = new List<object?>(updatePack.NewPropertyValues)
                {
                    updatePack.Id,
                    lastUser,
                };

                IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    updatedPropertyNames,
                    updatedPropertyValues,
                    number.ToString());

                totalParameters.AddRange(parameters);

                //sql
                //Remark: 由于packs中的pack可能是各种各样的，所以这里不能用模板，像Update那样
                string updatePropertiesSql = SqlHelper.CreateUpdatePropertiesIgnoreConflictCheckSql(modelDef, updatedPropertyNames, number);
                innerSqlBuilder.Append($"{updatePropertiesSql} {SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");

                number++;
            }

            string commandText = $@"{SqlHelper.Transaction_Begin(engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Create_Id(tempTableName, engineType)}
                                    {innerSqlBuilder}
                                    {SqlHelper.TempTable_Select_Id(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.Transaction_Commit(engineType)}";

            return new DbEngineCommand(commandText, totalParameters);
        }
    }
}
