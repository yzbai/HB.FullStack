#nullable enable

using HB.Framework.Common.Entities;
using System;
using System.Collections.Generic;
using System.Data;

namespace HB.Framework.Database.SQL
{
    internal interface ISQLBuilder
    {
        IDbCommand CreateAddCommand<T>(T entity) where T : Entity, new();
        IDbCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : Entity, new();

        IDbCommand CreateUpdateCommand<T>(WhereExpression<T> condition, T entity) where T : Entity, new();
        //IDbCommand CreateUpdateKeyCommand<T>(WhereExpression<T> condition, string[] keys, object[] values) where T : DatabaseEntity, new();

        IDbCommand CreateBatchAddCommand<T>(IEnumerable<T> entities) where T : Entity, new();

        /// <summary>
        /// 不允许重复删除
        /// </summary>
        IDbCommand CreateBatchDeleteCommand<T>(IEnumerable<T> entities) where T : Entity, new();

        /// <summary>
        /// 允许重复更新
        /// </summary>
        IDbCommand CreateBatchUpdateCommand<T>(IEnumerable<T> entities) where T : Entity, new();

        /// <exception cref="HB.Framework.Database.DatabaseException"></exception>
        IDbCommand CreateTableCommand(Type type, bool addDropStatement);

        IDbCommand CreateDeleteCommand<T>(WhereExpression<T> condition, string lastUser) where T : Entity, new();

        IDbCommand CreateRetrieveCommand<T>(SelectExpression<T>? selectCondition = null, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : Entity, new();

        IDbCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition)
            where T1 : Entity, new()
            where T2 : Entity, new();

        IDbCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition)
            where T1 : Entity, new()
            where T2 : Entity, new()
            where T3 : Entity, new();

        IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(SelectExpression<TSelect>? selectCondition, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition)
            where TSelect : Entity, new()
            where TFrom : Entity, new()
            where TWhere : Entity, new();


        IDbCommand CreateIsTableExistCommand(string databaseName, string tableName);

        IDbCommand CreateRetrieveSystemInfoCommand();

        IDbCommand CreateUpdateSystemVersionCommand(string databaseName, int version);

        SelectExpression<T> NewSelect<T>() where T : Entity, new();

        FromExpression<T> NewFrom<T>() where T : Entity, new();

        WhereExpression<T> NewWhere<T>() where T : Entity, new();
        IDbCommand CreateAddOrUpdateCommand<T>(T item) where T : Entity, new();
        IDbCommand CreateBatchAddOrUpdateCommand<T>(IEnumerable<T> items) where T : Entity, new();
    }
}