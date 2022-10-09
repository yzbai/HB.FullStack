
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
        FromExpression<T> From<T>(EngineType engineType) where T : DbModel, new();
        WhereExpression<T> Where<T>(EngineType engineType) where T : DbModel, new();
        WhereExpression<T> Where<T>(EngineType engineType, string sqlFilter, params object[] filterParams) where T : DbModel, new();
        WhereExpression<T> Where<T>(EngineType engineType, Expression<Func<T, bool>> predicate) where T : DbModel, new();

        EngineCommand CreateAddCommand<T>(EngineType engineType, DbModelDef modelDef, T model) where T : DbModel, new();
        EngineCommand CreateAddOrUpdateCommand<T>(EngineType engineType, DbModelDef modelDef, T model, bool returnModel) where T : DbModel, new();
        EngineCommand CreateBatchAddCommand<T>(EngineType engineType, DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new();
        EngineCommand CreateBatchAddOrUpdateCommand<T>(EngineType engineType, DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new();
        EngineCommand CreateBatchUpdateCommand<T>(EngineType engineType, DbModelDef modelDef, IEnumerable<T> models, IList<long> oldTimestamps, bool needTrans) where T : DbModel, new();
        EngineCommand CreateCountCommand<T>(EngineType engineType, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DbModel, new();

        EngineCommand CreateIsTableExistCommand(EngineType engineType, string databaseName, string tableName);
        EngineCommand CreateRetrieveCommand<T>(EngineType engineType, DbModelDef modelDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DbModel, new();
        EngineCommand CreateRetrieveCommand<T1, T2>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : DbModel, new()
            where T2 : DbModel, new();
        EngineCommand CreateRetrieveCommand<T1, T2, T3>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : DbModel, new()
            where T2 : DbModel, new()
            where T3 : DbModel, new();
        EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType engineType, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params DbModelDef[] returnModelDefs)
            where TSelect : DbModel, new()
            where TFrom : DbModel, new()
            where TWhere : DbModel, new();
        EngineCommand CreateSystemInfoRetrieveCommand(EngineType engineType);
        EngineCommand CreateSystemVersionUpdateCommand(EngineType engineType, string databaseName, int version);
        EngineCommand CreateTableCreateCommand(EngineType engineType, DbModelDef modelDef, bool addDropStatement, int varcharDefaultLength);
        EngineCommand CreateUpdateCommand<T>(EngineType engineType, DbModelDef modelDef, T model, long oldTimestamp) where T : DbModel, new();

        ///// <summary>
        ///// Version版本的乐观锁
        ///// </summary>
        //EngineCommand CreateUpdateFieldsUsingTimestampCompareCommand(EngineType engineType, DbModelDef modelDef, object id, long oldTimestamp, long newTimestamp, string lastUser, IList<string> propertyNames, IList<object?> propertyValues);

        EngineCommand CreateUpdatePropertiesCommand(
            EngineType engineType,
            DbModelDef modelDef,
            object id,
            IList<string> propertyNames,
            IList<object?> propertyValues,
            long? oldTimestamp,
            long? newTimestamp,
            string lastUser);

        EngineCommand CreateBatchUpdatePropertiesCommand(
            EngineType engineType,
            DbModelDef modelDef,
            IList<(object id, IList<string> propertyNames, IList<object?> propertyValues, long? oldTimestamp, long? newTimestamp)> modelChanges,
            string lastUser,
            bool needTrans);

        /// <summary>
        /// 新旧值版本的乐观锁
        /// </summary>
        EngineCommand CreateUpdatePropertiesUsingOldNewCompareCommand(
            EngineType engineType,
            DbModelDef modelDef,
            object id,
            IList<string> propertyNames,
            IList<object?> oldPropertyValues,
            IList<object?> newPropertyValues,
            long newTimestamp,
            string lastUser);

        EngineCommand CreateBatchUpdatePropertiesUsingOldNewCompareCommand(
            EngineType engineType,
            DbModelDef modelDef,
            IList<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues)> modelChanges,
            long newTimestamp,
            string lastUser,
            bool needTrans);

        EngineCommand CreateDeleteCommand(
            EngineType engineType,
            DbModelDef modelDef,
            object id,
            string lastUser,
            bool trulyDeleted,
            long? oldTimestamp,
            long? newTimestamp);

        EngineCommand CreateDeleteCommand<T>(
            EngineType engineType,
            DbModelDef modelDef,
            WhereExpression<T> whereExpression,
            string lastUser,
            bool trulyDeleted) where T : TimelessDbModel, new();

        public EngineCommand CreateBatchDeleteCommand(
            EngineType engineType,
            DbModelDef modelDef,
            IList<object> ids,
            IList<long?> oldTimestamps,
            IList<long?> newTimestamps,
            string lastUser,
            bool trulyDeleted,
            bool needTrans);

    }
}