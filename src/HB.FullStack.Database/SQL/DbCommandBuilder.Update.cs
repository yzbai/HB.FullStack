using System;
using System.Collections.Generic;

using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{


    /// <summary>
    /// DbCommandBuilder.Update
    /// </summary>
    internal partial class DbCommandBuilder
    {
        public DbEngineCommand CreateUpdateIgnoreConflictCheckCommand<T>(DbModelDef modelDef, T model) where T : IDbModel
        {
            return new DbEngineCommand(
                SqlHelper.CreateUpdateIgnoreConflictCheckSql(modelDef),
                model.ToDbParameters(modelDef, ModelDefFactory, null, 0));
        }
        
        public DbEngineCommand CreateBatchUpdateIgnoreConflictCheckCommand<T>(DbModelDef modelDef, IList<T> models, IList<long> oldTimestamps) where T : IDbModel
        {
            ThrowIf.Empty(models, nameof(models));

            return new DbEngineCommand(
                SqlHelper.CreateBatchUpdateIgnoreConflictCheckSql(modelDef, models.Count),
                models.ToDbParameters(modelDef, ModelDefFactory, null));
        }

        public DbEngineCommand CreateUpdateTimestampCommand<T>(DbModelDef modelDef, T model, long oldTimestamp) where T : IDbModel
        {
            IList<KeyValuePair<string, object>> paramters = model.ToDbParameters(modelDef, ModelDefFactory, null, 0);

            paramters.AddParameter(modelDef.TimestampPropertyDef!, oldTimestamp, SqlHelper.OLD_PARAMETER_SUFFIX, 0);

            return new DbEngineCommand(
                SqlHelper.CreateUpdateUsingTimestampSql(modelDef),
                paramters);
        }

        public DbEngineCommand CreateBatchUpdateTimestampCommand<T>(DbModelDef modelDef, IList<T> models, IList<long> oldTimestamps) where T : IDbModel
        {
            ThrowIf.NullOrEmpty(models, nameof(models));
            ThrowIf.CountNotEqual(models, oldTimestamps, "count not even.");

            var totalParameters = models.ToDbParameters(modelDef, ModelDefFactory, null);

            for (int i = 0; i < oldTimestamps.Count; ++i)
            {
                totalParameters.AddParameter(modelDef.TimestampPropertyDef!, oldTimestamps[i], SqlHelper.OLD_PARAMETER_SUFFIX, i);
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchUpdateUsingTimestampSql(modelDef, models.Count),
                totalParameters);
        }
    }
}
