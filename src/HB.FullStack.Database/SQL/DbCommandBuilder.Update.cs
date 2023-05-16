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
    /// <summary>
    /// DbCommandBuilder.Update
    /// </summary>
    internal partial class DbCommandBuilder
    {
        public DbEngineCommand CreateUpdateIgnoreConflictCheckCommand<T>(DbModelDef modelDef, T model) where T : BaseDbModel, new()
        {
            return new DbEngineCommand(
                SqlHelper.CreateUpdateIgnoreConflictCheckSql(modelDef), 
                model.ToDbParameters(modelDef, _modelDefFactory));
        }

        public DbEngineCommand CreateUpdateTimestampCommand<T>(DbModelDef modelDef, T model, long oldTimestamp) where T : BaseDbModel, new()
        {
            IList<KeyValuePair<string, object>> paramters = model.ToDbParameters(modelDef, _modelDefFactory);

            paramters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0", oldTimestamp));

            return new DbEngineCommand(
                SqlHelper.CreateUpdateUsingTimestampSql(modelDef), 
                paramters);
        }

        public DbEngineCommand CreateBatchUpdateTimestampCommand<T>(DbModelDef modelDef, IList<T> models, IList<long> oldTimestamps) where T : BaseDbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            int number = 0;
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

            foreach (T model in models)
            {
                parameters.AddRange(model.ToDbParameters(modelDef, _modelDefFactory, number));
                parameters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}", oldTimestamps[number]));

                number++;
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchUpdateUsingTimestampSql(modelDef, models.Count),
                parameters);
        }

        public DbEngineCommand CreateBatchUpdateIgnoreConflictCheckCommand<T>(DbModelDef modelDef, IList<T> models, IList<long> oldTimestamps) where T : BaseDbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            return new DbEngineCommand(
                SqlHelper.CreateBatchUpdateIgnoreConflictCheckSql(modelDef, models.Count),
                models.ToDbParameters(modelDef, _modelDefFactory));
        }
    }
}
