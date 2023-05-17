using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder
    {
        #region 更改 - AddOrUpdate

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update. 且Version不变,不增长
        /// </summary>
        public DbEngineCommand CreateAddOrUpdateCommand<T>(DbModelDef modelDef, T model, bool returnModel) where T : BaseDbModel, new()
        {
            if (!modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.Ignore))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow to use AddOrUpdate, which ignore conflict check.");
            }

            //只在客户端开放，因为不检查Version就update. 且Version不变,不增长
            return new DbEngineCommand(
                SqlHelper.CreateAddOrUpdateSql(modelDef, returnModel),
                model.ToDbParameters(modelDef, ModelDefFactory, null, 0));
        }

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update，并且无法更新models
        /// </summary>
        public DbEngineCommand CreateBatchAddOrUpdateCommand<T>(DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            DbEngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            //string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T model in models)
            {
                string addOrUpdateCommandText = SqlHelper.CreateAddOrUpdateSql(modelDef, false, number);

                parameters.AddRange(model.ToDbParameters(modelDef, ModelDefFactory, number));

                innerBuilder.Append(addOrUpdateCommandText);

                number++;
            }

            StringBuilder commandTextBuilder = new StringBuilder();

            if (needTrans)
            {
                commandTextBuilder.Append(SqlHelper.Transaction_Begin(engineType));
            }

            //commandTextBuilder.Append($"{SqlHelper.TempTable_Drop(tempTableName, engineType)}");
            //commandTextBuilder.Append($"{SqlHelper.TempTable_Create_Id(tempTableName, engineType)}");
            commandTextBuilder.Append(innerBuilder);
            //commandTextBuilder.Append($"{SqlHelper.TempTable_Drop(tempTableName, engineType)}");

            if (needTrans)
            {
                commandTextBuilder.Append(SqlHelper.Transaction_Commit(engineType));
            }

            return new DbEngineCommand(commandTextBuilder.ToString(), parameters);
        }

        #endregion
    }
}
