using System.Collections.Generic;
using System.Data;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Database.SQL
{
    internal interface IDbCommandBuilder
    {
        IDbCommand CreateAddCommand<T>(DatabaseEntityDef entityDef, T entity) where T : Entity, new();
        IDbCommand CreateBatchAddCommand<T>(DatabaseEntityDef entityDef, IEnumerable<T> entities) where T : Entity, new();
        IDbCommand CreateBatchDeleteCommand<T>(DatabaseEntityDef entityDef, IEnumerable<T> entities) where T : Entity, new();
        IDbCommand CreateBatchUpdateCommand<T>(DatabaseEntityDef entityDef, IEnumerable<T> entities) where T : Entity, new();
        IDbCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : Entity, new();
        IDbCommand CreateDeleteCommand<T>(DatabaseEntityDef entityDef, T entity) where T : Entity, new();
        IDbCommand CreateIsTableExistCommand(string databaseName, string tableName);
        IDbCommand CreateRetrieveCommand<T>(DatabaseEntityDef entityDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : Entity, new();
        IDbCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DatabaseEntityDef[] returnEntityDefs)
            where T1 : Entity, new()
            where T2 : Entity, new()
            where T3 : Entity, new();
        IDbCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DatabaseEntityDef[] returnEntityDefs)
            where T1 : Entity, new()
            where T2 : Entity, new();
        IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params DatabaseEntityDef[] returnEntityDefs)
            where TSelect : Entity, new()
            where TFrom : Entity, new()
            where TWhere : Entity, new();
        IDbCommand CreateSystemInfoRetrieveCommand();
        IDbCommand CreateSystemVersionUpdateCommand(string databaseName, int version);
        IDbCommand CreateTableCreateCommand(DatabaseEntityDef entityDef, bool addDropStatement);
        IDbCommand CreateUpdateCommand<T>(DatabaseEntityDef entityDef, T entity) where T : Entity, new();
        FromExpression<T> NewFrom<T>() where T : Entity, new();
        WhereExpression<T> NewWhere<T>() where T : Entity, new();
    }
}