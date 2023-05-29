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
        /// <summary>
        /// 只在客户端开放，因为不检查Version就update. 且Version不变,不增长
        /// </summary>
        public DbEngineCommand CreateAddOrUpdateCommand<T>(DbModelDef modelDef, T model, bool returnModel) where T : class, IDbModel
        {
            if (!modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Ignore))
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
        public DbEngineCommand CreateBatchAddOrUpdateCommand<T>(DbModelDef modelDef, IList<T> models) where T : class, IDbModel
        {
            if (!modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Ignore))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow to use AddOrUpdate, which ignore conflict check.");
            }

            ThrowIf.Empty(models, nameof(models));

            return new DbEngineCommand(
                SqlHelper.CreateBatchAddOrUpdateSql(modelDef, false, models.Count),
                models.ToDbParameters(modelDef, ModelDefFactory, null));
        }
    }
}
