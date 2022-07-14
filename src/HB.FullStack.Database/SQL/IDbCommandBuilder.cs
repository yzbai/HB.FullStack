

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.DatabaseModels;
using HB.FullStack.Database.SQL;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HB.FullStack.Database
{
    public interface IDbCommandBuilder
    {
        FromExpression<T> From<T>(EngineType engineType) where T : DBModel, new();
        WhereExpression<T> Where<T>(EngineType engineType) where T : DBModel, new();
        WhereExpression<T> Where<T>(EngineType engineType, string sqlFilter, params object[] filterParams) where T : DBModel, new();
        WhereExpression<T> Where<T>(EngineType engineType, Expression<Func<T, bool>> predicate) where T : DBModel, new();

        EngineCommand CreateAddCommand<T>(EngineType engineType, DBModelDef modelDef, T model) where T : DBModel, new();
        EngineCommand CreateAddOrUpdateCommand<T>(EngineType engineType, DBModelDef modelDef, T model, bool returnModel) where T : DBModel, new();
        EngineCommand CreateBatchAddCommand<T>(EngineType engineType, DBModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DBModel, new();
        EngineCommand CreateBatchAddOrUpdateCommand<T>(EngineType engineType, DBModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DBModel, new();
        EngineCommand CreateBatchDeleteCommand<T>(EngineType engineType, DBModelDef modelDef, IEnumerable<T> models, IList<long> oldTimestamps, bool needTrans) where T : DBModel, new();
        EngineCommand CreateBatchUpdateCommand<T>(EngineType engineType, DBModelDef modelDef, IEnumerable<T> models, IList<long> oldTimestamps, bool needTrans) where T : DBModel, new();
        EngineCommand CreateCountCommand<T>(EngineType engineType, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DBModel, new();
        EngineCommand CreateDeleteCommand<T>(EngineType engineType, DBModelDef modelDef, T model, long oldTimestamp) where T : DBModel, new();
        EngineCommand CreateDeleteCommand<T>(EngineType engineType, DBModelDef modelDef, WhereExpression<T> whereExpression) where T : TimeLessDBModel, new();
        EngineCommand CreateIsTableExistCommand(EngineType engineType, string databaseName, string tableName);
        EngineCommand CreateRetrieveCommand<T>(EngineType engineType, DBModelDef modelDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DBModel, new();
        EngineCommand CreateRetrieveCommand<T1, T2>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DBModelDef[] returnModelDefs)
            where T1 : DBModel, new()
            where T2 : DBModel, new();
        EngineCommand CreateRetrieveCommand<T1, T2, T3>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DBModelDef[] returnModelDefs)
            where T1 : DBModel, new()
            where T2 : DBModel, new()
            where T3 : DBModel, new();
        EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType engineType, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params DBModelDef[] returnModelDefs)
            where TSelect : DBModel, new()
            where TFrom : DBModel, new()
            where TWhere : DBModel, new();
        EngineCommand CreateSystemInfoRetrieveCommand(EngineType engineType);
        EngineCommand CreateSystemVersionUpdateCommand(EngineType engineType, string databaseName, int version);
        EngineCommand CreateTableCreateCommand(EngineType engineType, DBModelDef modelDef, bool addDropStatement, int varcharDefaultLength);
        EngineCommand CreateUpdateCommand<T>(EngineType engineType, DBModelDef modelDef, T model, long oldTimestamp) where T : DBModel, new();

        /// <summary>
        /// Version版本的乐观锁
        /// </summary>
        EngineCommand CreateUpdateFieldsUsingTimestampCompareCommand(EngineType engineType, DBModelDef modelDef, object id, long oldTimestamp, long newTimestamp, string lastUser, IList<string> propertyNames, IList<object?> propertyValues);

        /// <summary>
        /// 新旧值版本的乐观锁
        /// </summary>
        EngineCommand CreateUpdateFieldsUsingOldNewCompareCommand(EngineType engineType, DBModelDef modelDef, object id, long newTimestamp, string lastUser, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues);
    }
}