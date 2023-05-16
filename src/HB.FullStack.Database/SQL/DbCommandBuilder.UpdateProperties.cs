/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

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
                SqlHelper.CreateUpdatePropertiesUsingTimestampSql(modelDef, updatedPropertyNames),
                parameters);
        }

        public DbEngineCommand CreateBatchUpdatePropertiesTimestampCommand(DbModelDef modelDef, IList<TimestampUpdatePack> updatePacks, string lastUser)
        {
            modelDef.ThrowIfNotTimestamp();

            int number = 0;
            long curTimestamp = TimeUtil.Timestamp;
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            List<IList<string>> propertyNamesList = new List<IList<string>>();

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

                //TODO:整理OLD，NEw Paramters,重构
                parameters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}", updatePack.OldTimestamp.Value));

                totalParameters.AddRange(parameters);

                propertyNamesList.Add(updatedPropertyNames);

                number++;
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchUpdatePropertiesUsingTimestampSql(modelDef, propertyNamesList),
                totalParameters);
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
                $"{SqlHelper.NEW_PROPERTY_VALUE_SUFFIX}_0");

            //Remark:使用propertyNames而不是curPropertyNames

            return new DbEngineCommand(
                SqlHelper.CreateUpdatePropertiesUsingOldNewCompareSql(modelDef, updatePack.PropertyNames), 
                oldParameters.AddRange(newParameters));
        }

        public DbEngineCommand CreateBatchUpdatePropertiesOldNewCompareCommand(DbModelDef modelDef, IList<OldNewCompareUpdatePack> updatePacks, string lastUser)
        {
            //TODO:如果packs中的PropertyNames都相同，可以进一步提升性能
            ThrowIf.Empty(updatePacks, nameof(updatePacks));

            int number = 0;
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            List<IList<string>> propertyNamesList = new List<IList<string>>();

            foreach (OldNewCompareUpdatePack updatePack in updatePacks)
            {
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
                    $"{SqlHelper.NEW_PROPERTY_VALUE_SUFFIX}_{number}");

                totalParameters.AddRange(oldParameters);
                totalParameters.AddRange(newParameters);

                propertyNamesList.Add(updatePack.PropertyNames);

                number++;
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchUpdatePropertiesUsingOldNewCompareSql(modelDef, propertyNamesList),
                totalParameters);
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
                SqlHelper.CreateUpdatePropertiesIgnoreConflictCheckSql(modelDef, updatedPropertyNames),
                paramters);
        }

        public DbEngineCommand CreateBatchUpdatePropertiesIgnoreConflictCheckCommand(DbModelDef modelDef, IList<IgnoreConflictCheckUpdatePack> updatePacks, string lastUser)
        {
            int number = 0;
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            List<IList<string>> propertyNamesList = new List<IList<string>>();

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

                propertyNamesList.Add(updatedPropertyNames);

                number++;
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchUpdatePropertiesIgnoreConflictCheckSql(modelDef, propertyNamesList),
                totalParameters);
        }
    }
}