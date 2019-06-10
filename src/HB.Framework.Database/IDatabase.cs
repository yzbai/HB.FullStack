using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using System.Threading.Tasks;
using HB.Framework.Database.Engine;
using HB.Framework.Database.Transaction;

namespace HB.Framework.Database
{
    public interface IDatabase : IDatabaseAsync
    {
        //IDatabaseEngine DatabaseEngine { get; }

        IList<TSelect> Retrieve<TSelect, TFrom, TWhere>(
            SelectExpression<TSelect> selectCondition,
            FromExpression<TFrom> fromCondition,
            WhereExpression<TWhere> whereCondition,
            TransactionContext transContext)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new();

        //delete
        IList<T> Retrieve<T>(
            WhereExpression<T> whereCondition,
            TransactionContext transContext
            )
            where T : DatabaseEntity, new();

        IList<T> Retrieve<T>(
            Expression<Func<T, bool>> whereExpr,
            TransactionContext transContext
            )
            where T : DatabaseEntity, new();

        //modify
        IList<T> Retrieve<T>(
            FromExpression<T> fromCondition,
            WhereExpression<T> whereCondition,
            TransactionContext transContext
            )
            where T : DatabaseEntity, new();

        //Delete
        IList<T> Retrieve<T>(
            SelectExpression<T> selectCondition,
            FromExpression<T> fromCondition,
            WhereExpression<T> whereCondition,
            TransactionContext transContext
            )
            where T : DatabaseEntity, new();


        IList<Tuple<TSource, TTarget>> Retrieve<TSource, TTarget>(
            FromExpression<TSource> fromCondition,
            WhereExpression<TSource> whereCondition,
            TransactionContext transContext
            )
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();


        IList<Tuple<TSource, TTarget1, TTarget2>> Retrieve<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext )
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();
        string GetTableCreateStatement(Type type, bool addDropStatement);

        IList<T> RetrieveAll<T>(TransactionContext transContext ) where T : DatabaseEntity, new();


        long Count<T>(TransactionContext transContext ) where T : DatabaseEntity, new();
        long Count<T>(WhereExpression<T> condition, TransactionContext transContext ) where T : DatabaseEntity, new();
        long Count<T>(Expression<Func<T, bool>> whereExpr, TransactionContext transContext ) where T : DatabaseEntity, new();
        long Count<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext ) where T : DatabaseEntity, new();
        long Count<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext ) where T : DatabaseEntity, new();


        IList<T> Page<T>(long pageNumber, long perPageCount, TransactionContext transContext ) where T : DatabaseEntity, new();
        IList<T> Page<T>(WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext ) where T : DatabaseEntity, new();
        IList<T> Page<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, TransactionContext transContext ) where T : DatabaseEntity, new();
        IList<T> Page<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext ) where T : DatabaseEntity, new();
        IList<T> Page<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext) where T : DatabaseEntity, new();
        IList<Tuple<TSource, TTarget>> Page<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        IList<Tuple<TSource, TTarget1, TTarget2>> Page<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext )
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();


        T Scalar<T>(WhereExpression<T> whereCondition, TransactionContext transContext ) where T : DatabaseEntity, new();
        T Scalar<T>(long id, TransactionContext transContext ) where T : DatabaseEntity, new();
        T Scalar<T>(Expression<Func<T, bool>> whereExpr, TransactionContext transContext ) where T : DatabaseEntity, new();
        T Scalar<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext ) where T : DatabaseEntity, new();
        T Scalar<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext ) where T : DatabaseEntity, new();
        Tuple<TSource, TTarget> Scalar<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext )
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        Tuple<TSource, TTarget1, TTarget2> Scalar<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext )
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();



        DatabaseResult Add<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new();
        DatabaseResult Delete<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new();
        DatabaseResult Update<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new();

        //DatabaseResult Set<T>(T item, DatabaseTransactionContext transactionContext = null) where T : DatabaseEntity, new();

        DatabaseResult BatchAdd<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new();
        DatabaseResult BatchDelete<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new();
        DatabaseResult BatchUpdate<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new();

        SelectExpression<T> Select<T>() where T : DatabaseEntity, new();
        FromExpression<T> From<T>() where T : DatabaseEntity, new();
        WhereExpression<T> Where<T>() where T : DatabaseEntity, new();

        //#region 事务

        //TransactionContext BeginTransaction<T>(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : DatabaseEntity;

        ///// <summary>
        ///// 提交事务
        ///// </summary>
        //void Commit(TransactionContext context);

        ///// <summary>
        ///// 回滚事务
        ///// </summary>
        //void Rollback(TransactionContext context);

        //#endregion
    }
}