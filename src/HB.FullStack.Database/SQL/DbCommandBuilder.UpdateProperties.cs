using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder
    {
        #region 更改 - UpdateProperties

        public DbEngineCommand CreateUpdatePropertiesTimestampCommand(DbModelDef modelDef, TimestampUpdatePack updatePack, string lastUser)
        {
            modelDef.ThrowIfNotTimestamp();

            DbEngineType engineType = modelDef.EngineType;

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
               TimeUtil.Timestamp
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

            long newTimestamp = TimeUtil.Timestamp;

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
                    newTimestamp
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

        public DbEngineCommand CreateUpdatePropertie sTimelessCommand(DbModelDef modelDef, OldNewCompareUpdatePack updatePack, string lastUser)
        {
            if (modelDef.IsTimestamp)
            {
                throw DbExceptions.UpdatePropertiesMethodWrong("TimestampDBModel 应该使用 Timestamp解决冲突", updatePack.PropertyNames, modelDef);
            }

            List<string> curPropertyNames = new List<string>(updatePack.PropertyNames);
            List<object?> curNewPropertyValues = new List<object?>(updatePack.NewPropertyValues);

            var oldParameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                curPropertyNames,
                updatePack.OldPropertyValues,
                $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0");

            curPropertyNames.Add(nameof(TimelessLongIdDbModel.Id));
            curPropertyNames.Add(nameof(TimelessLongIdDbModel.LastUser));
            curNewPropertyValues.Add(updatePack.Id);
            curNewPropertyValues.Add(lastUser);

            var newParameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                curPropertyNames,
                curNewPropertyValues,
                $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_0");

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(oldParameters);
            parameters.AddRange(newParameters);

            //使用propertyNames而不是curPropertyNames
            string sql = GetCachedSql(SqlType.UpdatePropertiesTimeless, new DbModelDef[] { modelDef }, updatePack.PropertyNames);

            return new DbEngineCommand(sql, parameters);
        }

        public DbEngineCommand CreateBatchUpdatePropertiesTimelessCommand(DbModelDef modelDef, IList<OldNewCompareUpdatePack> updatePacks, string lastUser, bool needTrans)
        {
            if (modelDef.IsTimestamp)
            {
                throw DbExceptions.UpdatePropertiesMethodWrong("TimestampDBModel 应该使用 Timestamp解决冲突", updatePacks[0].PropertyNames, modelDef);
            }

            ThrowIf.Empty(updatePacks, nameof(updatePacks));

            DbEngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (OldNewCompareUpdatePack updatePack in updatePacks)
            {
                List<string> curPropertyNames = new List<string>(updatePack.PropertyNames);
                List<object?> curNewPropertyValues = new List<object?>(updatePack.NewPropertyValues);

                #region Parameters

                var oldParameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    curPropertyNames,
                    updatePack.OldPropertyValues,
                    $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}");

                curPropertyNames.Add(nameof(TimestampLongIdDbModel.Id));
                curPropertyNames.Add(nameof(DbModel.LastUser));
                curNewPropertyValues.Add(updatePack.Id);
                curNewPropertyValues.Add(lastUser);

                var newParameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    curPropertyNames,
                    curNewPropertyValues,
                    $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_{number}");

                totalParameters.AddRange(oldParameters);
                totalParameters.AddRange(newParameters);

                #endregion

                string sql = SqlHelper.CreateUpdatePropertiesUsingCompareSql(modelDef, updatePack.PropertyNames, number);

#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{sql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{sql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
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

        #endregion
    }
}
