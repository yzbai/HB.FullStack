

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
        FromExpression<T> From<T>(EngineType engineType) where T : DatabaseModel, new();
        WhereExpression<T> Where<T>(EngineType engineType) where T : DatabaseModel, new();
        WhereExpression<T> Where<T>(EngineType engineType, string sqlFilter, params object[] filterParams) where T : DatabaseModel, new();
        WhereExpression<T> Where<T>(EngineType engineType, Expression<Func<T, bool>> predicate) where T : DatabaseModel, new();

        EngineCommand CreateAddCommand<T>(EngineType engineType, DatabaseModelDef modelDef, T model) where T : DatabaseModel, new();
        EngineCommand CreateAddOrUpdateCommand<T>(EngineType engineType, DatabaseModelDef modelDef, T model) where T : DatabaseModel, new();
        EngineCommand CreateBatchAddCommand<T>(EngineType engineType, DatabaseModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DatabaseModel, new();
        EngineCommand CreateBatchAddOrUpdateCommand<T>(EngineType engineType, DatabaseModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DatabaseModel, new();
        EngineCommand CreateBatchDeleteCommand<T>(EngineType engineType, DatabaseModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DatabaseModel, new();
        EngineCommand CreateBatchUpdateCommand<T>(EngineType engineType, DatabaseModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DatabaseModel, new();
        EngineCommand CreateCountCommand<T>(EngineType engineType, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DatabaseModel, new();
        EngineCommand CreateDeleteCommand<T>(EngineType engineType, DatabaseModelDef modelDef, T model) where T : DatabaseModel, new();
        EngineCommand CreateDeleteCommand<T>(EngineType engineType, DatabaseModelDef modelDef, WhereExpression<T> whereExpression) where T : DatabaseModel, new();
        EngineCommand CreateIsTableExistCommand(EngineType engineType, string databaseName, string tableName);
        EngineCommand CreateRetrieveCommand<T>(EngineType engineType, DatabaseModelDef modelDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DatabaseModel, new();
        EngineCommand CreateRetrieveCommand<T1, T2>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DatabaseModelDef[] returnModelDefs)
            where T1 : DatabaseModel, new()
            where T2 : DatabaseModel, new();
        EngineCommand CreateRetrieveCommand<T1, T2, T3>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DatabaseModelDef[] returnModelDefs)
            where T1 : DatabaseModel, new()
            where T2 : DatabaseModel, new()
            where T3 : DatabaseModel, new();
        EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType engineType, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params DatabaseModelDef[] returnModelDefs)
            where TSelect : DatabaseModel, new()
            where TFrom : DatabaseModel, new()
            where TWhere : DatabaseModel, new();
        EngineCommand CreateSystemInfoRetrieveCommand(EngineType engineType);
        EngineCommand CreateSystemVersionUpdateCommand(EngineType engineType, string databaseName, int version);
        EngineCommand CreateTableCreateCommand(EngineType engineType, DatabaseModelDef modelDef, bool addDropStatement, int varcharDefaultLength);
        EngineCommand CreateUpdateCommand<T>(EngineType engineType, DatabaseModelDef modelDef, T model) where T : DatabaseModel, new();
        
        /// <summary>
        /// Version版本的乐观锁
        /// </summary>
        EngineCommand CreateUpdateFieldsCommand(EngineType engineType, DatabaseModelDef modelDef, object id, int updateToVersion, string lastUser, IList<string> propertyNames, IList<object?> propertyValues);

        /// <summary>
        /// 新旧值版本的乐观锁
        /// </summary>
        EngineCommand CreateUpdateFieldsCommand(EngineType engineType, DatabaseModelDef modelDef, object id, string lastUser, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues);
    }
}