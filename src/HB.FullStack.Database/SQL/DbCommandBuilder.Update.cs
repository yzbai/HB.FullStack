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
        #region 更改 - Update

        public DbEngineCommand CreateUpdateCommand<T>(DbModelDef modelDef, T model, long? oldTimestamp, DbConflictCheckMethods conflictCheckMethods) where T : BaseDbModel, new()
        {
            switch (conflictCheckMethods)
            {
                case DbConflictCheckMethods.Ignore:
                {
                    IList<KeyValuePair<string, object>> paramters = model.ToDbParameters(modelDef, _modelDefFactory);

                    string sql = GetCachedSql(SqlType.UpdateIgnoreConflictCheck, new DbModelDef[] { modelDef });
                    
                    return new DbEngineCommand(sql, paramters);
                }
                case DbConflictCheckMethods.OldNewValueCompare:
                    break;
                case DbConflictCheckMethods.Timestamp:
                {
                    IList<KeyValuePair<string, object>> paramters = model.ToDbParameters(modelDef, _modelDefFactory);

                    if (oldTimestamp != null)
                    {
                        paramters.Add(new KeyValuePair<string, object>($"{SqlHelper.GetParameterized(nameof(ITimestamp.Timestamp))}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0", oldTimestamp.Value));
                    }
                    break;
                }
                default:
                    throw DbExceptions.ConflictCheckMethodError($"{modelDef.ModelFullName} has incorrent ConflictCheckMethod:{conflictCheckMethods}");
            }
        }

        public DbEngineCommand CreateBatchUpdateCommand<T>(DbModelDef modelDef, IEnumerable<T> models, IList<long> oldTimestamps, bool needTrans) where T : BaseDbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            if (modelDef.IsTimestamp)
            {
                ThrowIf.NotEqual(models.Count(), oldTimestamps.Count, nameof(models), nameof(oldTimestamps));
            }

            DbEngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            DbModelPropertyDef? timestampProperty = modelDef.IsTimestamp ? modelDef.GetDbPropertyDef(nameof(ITimestamp.Timestamp))! : null;

            foreach (T model in models)
            {
                string updateCommandText = SqlHelper.CreateUpdateModelSql(modelDef, number);

                parameters.AddRange(model.ToDbParameters(modelDef, _modelDefFactory, number));

                //这里要添加 一些参数值，参考update
                if (modelDef.IsTimestamp)
                {
                    parameters.Add(new KeyValuePair<string, object>($"{timestampProperty!.DbParameterizedName}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}", oldTimestamps[number]));
                }

#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{updateCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{updateCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
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

            return new DbEngineCommand(commandText, parameters);
        }

        #endregion
    }
}
