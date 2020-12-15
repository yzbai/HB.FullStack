using System.Collections.Generic;
using System.Data;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    internal interface IDbCommandBuilder
    {
        IDbCommand CreateAddCommand<T>(EntityDef entityDef, T entity) where T : Entity, new();

        IDbCommand CreateBatchAddCommand<T>(EntityDef entityDef, IEnumerable<T> entities) where T : Entity, new();

        IDbCommand CreateBatchDeleteCommand<T>(EntityDef entityDef, IEnumerable<T> entities) where T : Entity, new();

        IDbCommand CreateBatchUpdateCommand<T>(EntityDef entityDef, IEnumerable<T> entities) where T : Entity, new();

        IDbCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : Entity, new();

        IDbCommand CreateDeleteCommand<T>(EntityDef entityDef, T entity) where T : Entity, new();

        IDbCommand CreateIsTableExistCommand(string databaseName, string tableName);

        IDbCommand CreateRetrieveCommand<T>(EntityDef entityDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : Entity, new();

        IDbCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : Entity, new()
            where T2 : Entity, new()
            where T3 : Entity, new();

        IDbCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : Entity, new()
            where T2 : Entity, new();

        IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params EntityDef[] returnEntityDefs)
            where TSelect : Entity, new()
            where TFrom : Entity, new()
            where TWhere : Entity, new();

        IDbCommand CreateSystemInfoRetrieveCommand();

        IDbCommand CreateSystemVersionUpdateCommand(string databaseName, int version);

        IDbCommand CreateTableCreateCommand(EntityDef entityDef, bool addDropStatement);

        IDbCommand CreateUpdateCommand<T>(EntityDef entityDef, T entity) where T : Entity, new();

        FromExpression<T> NewFrom<T>() where T : Entity, new();

        WhereExpression<T> NewWhere<T>() where T : Entity, new();
    }
}