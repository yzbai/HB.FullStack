﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder
    {
        public DbEngineCommand CreateAddCommand<T>(DbModelDef modelDef, T model) where T : BaseDbModel, new()
        {
            return new DbEngineCommand(
                SqlHelper.CreateInsertSql(modelDef),
                model.ToDbParameters(modelDef, _modelDefFactory));
        }

        public DbEngineCommand CreateBatchAddCommand<T>(DbModelDef modelDef, IList<T> models) where T : BaseDbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            return new DbEngineCommand(
                SqlHelper.CreateBatchInsertSql(modelDef, models.Count),
                models.ToDbParameters(modelDef, _modelDefFactory));
        }
    }
}
