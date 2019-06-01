using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace HB.Framework.Database.SQL
{
    internal interface ISQLBuilder
    {
        IDbCommand CreateAddCommand<T>(T entity, string lastUser) where T : DatabaseEntity, new();
        IDbCommand CreateCountCommand<T>(FromExpression<T> fromCondition = null, WhereExpression<T> whereCondition = null) where T : DatabaseEntity, new();
        
        IDbCommand CreateUpdateCommand<T>(WhereExpression<T> condition, T entity, string lastUser) where T : DatabaseEntity, new();
        IDbCommand CreateUpdateKeyCommand<T>(WhereExpression<T> condition, string[] keys, object[] values, string lastUser) where T : DatabaseEntity, new();

        IDbCommand CreateBatchAddStatement<T>(IEnumerable<T> entities, string lastUser) where T : DatabaseEntity, new();

        /// <summary>
        /// 不允许重复删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        IDbCommand CreateBatchDeleteStatement<T>(IEnumerable<T> entities, string lastUser) where T : DatabaseEntity, new();

        /// <summary>
        /// 允许重复更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        IDbCommand CreateBatchUpdateStatement<T>(IEnumerable<T> entities, string lastUser) where T : DatabaseEntity, new();
        string GetTableCreateStatement(Type type, bool addDropStatement);
        IDbCommand GetDeleteCommand<T>(WhereExpression<T> condition, string lastUser) where T : DatabaseEntity, new();

        SelectExpression<T> NewSelect<T>() where T : DatabaseEntity, new();

        FromExpression<T> NewFrom<T>() where T : DatabaseEntity, new();

        WhereExpression<T> NewWhere<T>() where T : DatabaseEntity, new();

        IDbCommand CreateRetrieveCommand<T>(SelectExpression<T> selectCondition = null, FromExpression<T> fromCondition = null, WhereExpression<T> whereCondition = null) 
            where T : DatabaseEntity, new();

        IDbCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new();

        IDbCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
            where T3 : DatabaseEntity, new();

        IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(SelectExpression<TSelect> selectCondition, FromExpression<TFrom> fromCondition, WhereExpression<TWhere> whereCondition)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new();
    }
}