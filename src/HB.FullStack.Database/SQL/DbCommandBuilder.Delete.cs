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

using Microsoft;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder
    {
        public DbEngineCommand CreateDeleteIgnoreConflictCheckCommand(DbModelDef modelDef, object id, string lastUser, bool trulyDelete, long? newTimestamp = null)
        {
            //checks
            newTimestamp ??= TimeUtil.Timestamp;

            //parameters
            var propertyNames = new List<string> { nameof(DbModel<long>.Id) };
            var propertyValues = new List<object?> { id };

            var parameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, propertyNames, propertyValues, null, 0);

            //new parameters
            var newProperyNames = new List<string> { nameof(IDbModel.LastUser) };
            var newPropertyValues = new List<object?> { lastUser };

            if (modelDef.IsTimestamp)
            {
                newProperyNames.Add(nameof(ITimestamp.Timestamp));
                newPropertyValues.Add(newTimestamp);
            }

            var newParameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, newProperyNames, newPropertyValues, SqlHelper.NEW_PARAMETER_SUFFIX, 0);

            //return
            return new DbEngineCommand(
                SqlHelper.CreateDeleteIgnoreConflictCheckSql(modelDef, trulyDelete),
                parameters,
                newParameters);
        }

        public DbEngineCommand CreateBatchDeleteIgnoreConflictCheckCommand(DbModelDef modelDef, IList<object> ids, string lastUser, bool trulyDeleted, long? newTimestamp = null)
        {
            //Checks
            ThrowIf.NullOrEmpty(ids, nameof(ids));
            newTimestamp ??= TimeUtil.Timestamp;

            IList<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();

            var propertyNames = new List<string> { nameof(DbModel<long>.Id) };
            var newProperyNames = new List<string> { nameof(IDbModel.LastUser) };

            if (modelDef.IsTimestamp)
            {
                newProperyNames.Add(nameof(ITimestamp.Timestamp));
            }

            int number = 0;

            foreach (var id in ids)
            {
                //parameters
                var propertyValues = new List<object?> { id };

                var parameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, propertyNames, propertyValues, null, number);

                totalParameters.AddRange(parameters);

                //new parameters
                var newPropertyValues = new List<object?> { lastUser };

                if (modelDef.IsTimestamp)
                {
                    newPropertyValues.Add(newTimestamp);
                }

                var newParameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, newProperyNames, newPropertyValues, SqlHelper.NEW_PARAMETER_SUFFIX, number);

                totalParameters.AddRange(newParameters);

                number++;
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchDeleteIgnoreConflictCheckSql(modelDef, trulyDeleted, ids.Count),
                totalParameters);
        }

        public DbEngineCommand CreateDeleteTimestampCommand(DbModelDef modelDef, object id, long timestamp, string lastUser, bool trulyDelete, long? newTimestamp = null)
        {
            //checks
            newTimestamp ??= TimeUtil.Timestamp;

            //parameters
            List<string> propertyNames = new List<string> { nameof(DbModel<long>.Id), nameof(ITimestamp.Timestamp) };
            List<object?> propertyValues = new List<object?> { id, timestamp };
            var parameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, propertyNames, propertyValues, null, 0);

            //new parameters
            List<string> newPropertyNames = new List<string> { nameof(IDbModel.LastUser), nameof(ITimestamp.Timestamp) };
            List<object?> newPropertyValues = new List<object?> { lastUser, newTimestamp };
            var newParameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, newPropertyNames, newPropertyValues, SqlHelper.NEW_PARAMETER_SUFFIX, 0);

            //return
            return new DbEngineCommand(
                SqlHelper.CreateDeleteUsingTimestampSql(modelDef, trulyDelete),
                parameters,
                newParameters);
        }

        public DbEngineCommand CreateBatchDeleteTimestampCommand(DbModelDef modelDef, IList<object> ids, IList<long> timestamps, string lastUser, bool trulyDelete, long? newTimestamp = null)
        {
            //checks
            ThrowIf.NullOrEmpty(ids, nameof(ids));
            ThrowIf.CountNotEqual(ids, timestamps, "");
            newTimestamp ??= TimeUtil.Timestamp;

            IList<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();

            var propertyNames = new List<string> { nameof(DbModel<long>.Id), nameof(ITimestamp.Timestamp) };
            var newPropertyNames = new List<string> { nameof(IDbModel.LastUser), nameof(ITimestamp.Timestamp) };

            int number = 0;

            foreach (var id in ids)
            {
                //parameters
                List<object?> propertyValues = new List<object?> { id, timestamps[number] };

                var parameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, propertyNames, propertyValues, null, number);

                totalParameters.AddRange(parameters);

                //new parameters
                var newPropertyValues = new List<object?> { lastUser, newTimestamp };
                var newParameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, newPropertyNames, newPropertyValues, SqlHelper.NEW_PARAMETER_SUFFIX, number);

                totalParameters.AddRange(newParameters);

                number++;
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchDeleteUsingTimestampSql(modelDef, trulyDelete, ids.Count),
                totalParameters);
        }

        public DbEngineCommand CreateDeleteOldNewCompareCommand<T>(DbModelDef modelDef, T model, string lastUser, bool trulyDelete, long? newTimestamp = null) where T : class, IDbModel
        {
            //Check
            newTimestamp ??= TimeUtil.Timestamp;

            //parameters
            var parameters = model.ToDbParameters(modelDef, ModelDefFactory, null, 0);

            //new parameters
            List<string> newPropertyNames = new List<string> { nameof(IDbModel.LastUser) };
            List<object?> newPropertyValues = new List<object?> { lastUser };

            if (modelDef.IsTimestamp)
            {
                newPropertyNames.Add(nameof(ITimestamp.Timestamp));
                newPropertyValues.Add(newTimestamp);
            }

            var newParameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, newPropertyNames, newPropertyValues, SqlHelper.NEW_PARAMETER_SUFFIX, 0);

            //return
            return new DbEngineCommand(
                SqlHelper.CreateDeleteUsingOldNewCompareSql(modelDef, trulyDelete),
                parameters,
                newParameters);
        }

        public DbEngineCommand CreateBatchDeleteOldNewCompareCommand<T>(DbModelDef modelDef, IList<T> models, string lastUser, bool trulyDelete, long? newTimestamp = null) where T : class, IDbModel
        {
            //checks
            ThrowIf.NullOrEmpty(models, nameof(models));
            newTimestamp ??= TimeUtil.Timestamp;

            IList<KeyValuePair<string, object>> totalParameters = models.ToDbParameters(modelDef, ModelDefFactory, null);

            var newPropertyNames = new List<string> { nameof(IDbModel.LastUser) };

            if (modelDef.IsTimestamp)
            {
                newPropertyNames.Add(nameof(ITimestamp.Timestamp));
            }

            for (int i = 0; i < models.Count; ++i)
            {
                List<object?> newPropertyValues = new List<object?> { lastUser };

                if (modelDef.IsTimestamp)
                {
                    newPropertyValues.Add(newTimestamp);
                }

                var newParameters = DbModelConvert.PropertyValuesToParameters(modelDef, ModelDefFactory, newPropertyNames, newPropertyValues, SqlHelper.NEW_PARAMETER_SUFFIX, i);

                totalParameters.AddRange(newParameters);
            }

            return new DbEngineCommand(
                SqlHelper.CreateBatchDeleteUsingOldNewCompareSql(modelDef, trulyDelete, models.Count),
                totalParameters);
        }

        public DbEngineCommand CreateDeleteConditonCommand<T>(
            DbModelDef modelDef,
            WhereExpression<T> whereExpression,
            string lastUser,
            bool trulyDeleted) where T : class, IDbModel
        {
            Requires.NotNull(whereExpression, nameof(whereExpression));

            IList<KeyValuePair<string, object>> parameters = whereExpression.GetParameters();

            parameters.AddParameter(modelDef.LastUserPropertyDef, lastUser, SqlHelper.NEW_PARAMETER_SUFFIX, 0);

            if (modelDef.IsTimestamp)
            {
                parameters.AddParameter(modelDef.TimestampPropertyDef!, TimeUtil.Timestamp, SqlHelper.NEW_PARAMETER_SUFFIX, 0);
            }

            string sql = SqlHelper.CreateDeleteUsingConditionSql(modelDef, whereExpression, trulyDeleted);

            return new DbEngineCommand(sql, parameters);
        }

    }
}

