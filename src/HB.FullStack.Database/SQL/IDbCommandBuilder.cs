

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HB.FullStack.Database
{
    public interface IDbCommandBuilder
    {
        FromExpression<T> From<T>(EngineType engineType) where T : DatabaseEntity, new();
        WhereExpression<T> Where<T>(EngineType engineType) where T : DatabaseEntity, new();
        WhereExpression<T> Where<T>(EngineType engineType, string sqlFilter, params object[] filterParams) where T : DatabaseEntity, new();
        WhereExpression<T> Where<T>(EngineType engineType, Expression<Func<T, bool>> predicate) where T : DatabaseEntity, new();

        EngineCommand CreateAddCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new();
        EngineCommand CreateAddOrUpdateCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new();
        EngineCommand CreateBatchAddCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities, bool needTrans) where T : DatabaseEntity, new();
        EngineCommand CreateBatchAddOrUpdateCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities, bool needTrans) where T : DatabaseEntity, new();
        EngineCommand CreateBatchDeleteCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities, bool needTrans) where T : DatabaseEntity, new();
        EngineCommand CreateBatchUpdateCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities, bool needTrans) where T : DatabaseEntity, new();
        EngineCommand CreateCountCommand<T>(EngineType engineType, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DatabaseEntity, new();
        EngineCommand CreateDeleteCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new();
        EngineCommand CreateDeleteCommand<T>(EngineType engineType, EntityDef entityDef, WhereExpression<T> whereExpression) where T : DatabaseEntity, new();
        EngineCommand CreateIsTableExistCommand(EngineType engineType, string databaseName, string tableName);
        EngineCommand CreateRetrieveCommand<T>(EngineType engineType, EntityDef entityDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : DatabaseEntity, new();
        EngineCommand CreateRetrieveCommand<T1, T2>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new();
        EngineCommand CreateRetrieveCommand<T1, T2, T3>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
            where T3 : DatabaseEntity, new();
        EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType engineType, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params EntityDef[] returnEntityDefs)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new();
        EngineCommand CreateSystemInfoRetrieveCommand(EngineType engineType);
        EngineCommand CreateSystemVersionUpdateCommand(EngineType engineType, string databaseName, int version);
        EngineCommand CreateTableCreateCommand(EngineType engineType, EntityDef entityDef, bool addDropStatement, int varcharDefaultLength);
        EngineCommand CreateUpdateCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new();
        
        /// <summary>
        /// Version版本的乐观锁
        /// </summary>
        EngineCommand CreateUpdateFieldsCommand(EngineType engineType, EntityDef entityDef, object id, int updateToVersion, string lastUser, IList<string> propertyNames, IList<object?> propertyValues);

        /// <summary>
        /// 新旧值版本的乐观锁
        /// </summary>
        EngineCommand CreateUpdateFieldsCommand(EngineType engineType, EntityDef entityDef, object id, string lastUser, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues);
    }
}