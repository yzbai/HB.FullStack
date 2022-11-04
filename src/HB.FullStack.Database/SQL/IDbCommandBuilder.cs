
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    public interface IDbCommandBuilder
    {
        FromExpression<T> From<T>() where T : DbModel, new();
        WhereExpression<T> Where<T>() where T : DbModel, new();
        WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DbModel, new();
        WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DbModel, new();

        EngineCommand CreateAddCommand<T>(DbModelDef modelDef, T model) where T : DbModel, new();
        EngineCommand CreateAddOrUpdateCommand<T>(DbModelDef modelDef, T model, bool returnModel) where T : DbModel, new();
        EngineCommand CreateBatchAddCommand<T>(DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new();
        EngineCommand CreateBatchAddOrUpdateCommand<T>(DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new();
        EngineCommand CreateBatchUpdateCommand<T>(DbModelDef modelDef, IEnumerable<T> models, IList<long> oldTimestamps, bool needTrans) where T : DbModel, new();
        EngineCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DbModel, new();

        /// <summary>
        /// 当前Connection下
        /// </summary>
        EngineCommand CreateIsTableExistCommand(EngineType engineType, string tableName);
        EngineCommand CreateRetrieveCommand<T>(DbModelDef modelDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DbModel, new();
        EngineCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : DbModel, new()
            where T2 : DbModel, new();
        EngineCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : DbModel, new()
            where T2 : DbModel, new()
            where T3 : DbModel, new();
        EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(
            FromExpression<TFrom>? fromCondition,
            WhereExpression<TWhere>? whereCondition,
            params DbModelDef[] returnModelDefs)
            where TSelect : DbModel, new()
            where TFrom : DbModel, new()
            where TWhere : DbModel, new();
        EngineCommand CreateSystemInfoRetrieveCommand(EngineType engineType);

        /// <summary>
        /// 如果tb_sys_info不存在则创建
        /// </summary>
        EngineCommand CreateSystemVersionSetCommand(EngineType engineType, string dbSchema, int version);
        EngineCommand CreateTableCreateCommand(DbModelDef modelDef, bool addDropStatement, int varcharDefaultLength);
        EngineCommand CreateUpdateCommand<T>(DbModelDef modelDef, T model, long oldTimestamp) where T : DbModel, new();

        EngineCommand CreateUpdatePropertiesCommand(
            DbModelDef modelDef,
            object id,
            IList<string> propertyNames,
            IList<object?> propertyValues,
            long? oldTimestamp,
            long? newTimestamp,
            string lastUser);

        EngineCommand CreateBatchUpdatePropertiesCommand(
            DbModelDef modelDef,
            IList<(object id, IList<string> propertyNames, IList<object?> propertyValues, long? oldTimestamp, long? newTimestamp)> modelChanges,
            string lastUser,
            bool needTrans);

        /// <summary>
        /// 新旧值版本的乐观锁
        /// </summary>
        EngineCommand CreateUpdatePropertiesUsingOldNewCompareCommand(
            DbModelDef modelDef,
            object id,
            IList<string> propertyNames,
            IList<object?> oldPropertyValues,
            IList<object?> newPropertyValues,
            long newTimestamp,
            string lastUser);

        EngineCommand CreateBatchUpdatePropertiesUsingOldNewCompareCommand(
            DbModelDef modelDef,
            IList<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues, long newTimestamp)> modelChanges,
            string lastUser,
            bool needTrans);

        EngineCommand CreateDeleteCommand(
            DbModelDef modelDef,
            object id,
            string lastUser,
            bool trulyDeleted,
            long? oldTimestamp,
            long? newTimestamp);

        EngineCommand CreateDeleteCommand<T>(
            DbModelDef modelDef,
            WhereExpression<T> whereExpression,
            string lastUser,
            bool trulyDeleted) where T : TimelessDbModel, new();

        public EngineCommand CreateBatchDeleteCommand(
            DbModelDef modelDef,
            IList<object> ids,
            IList<long?> oldTimestamps,
            IList<long?> newTimestamps,
            string lastUser,
            bool trulyDeleted,
            bool needTrans);

    }
}