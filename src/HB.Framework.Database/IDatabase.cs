using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using System.Threading.Tasks;
using HB.Framework.Database.Engine;

namespace HB.Framework.Database
{
    public interface IDatabase : IDatabaseAsync
    {
        IDatabaseEngine DatabaseEngine { get; }

        IList<TSelect> Retrieve<TSelect, TFrom, TWhere>(
            SelectExpression<TSelect> selectCondition, 
            FromExpression<TFrom> fromCondition, 
            WhereExpression<TWhere> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new();

        //delete
        IList<T> Retrieve<T>(
            WhereExpression<T> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false) 
            where T : DatabaseEntity, new();

        IList<T> Retrieve<T>(
            Expression<Func<T, bool>> whereExpr, 
            DbTransactionContext transContext = null, 
            bool useMaster = false) 
            where T : DatabaseEntity, new();

        //modify
        IList<T> Retrieve<T>(
            FromExpression<T> fromCondition, 
            WhereExpression<T> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false) 
            where T : DatabaseEntity, new();

        //Delete
        IList<T> Retrieve<T>(
            SelectExpression<T> selectCondition, 
            FromExpression<T> fromCondition, 
            WhereExpression<T> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false) 
            where T : DatabaseEntity, new();


        IList<Tuple<TSource, TTarget>> Retrieve<TSource, TTarget>(
            FromExpression<TSource> fromCondition, 
            WhereExpression<TSource> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();


        IList<Tuple<TSource, TTarget1, TTarget2>> Retrieve<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();


        IList<T> RetrieveAll<T>(DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();


        long Count<T>(DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        long Count<T>(WhereExpression<T> condition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        long Count<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        long Count<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        long Count<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();


        IList<T> Page<T>(long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<T> Page<T>(WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<T> Page<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<T> Page<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<T> Page<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<Tuple<TSource, TTarget>> Page<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        IList<Tuple<TSource, TTarget1, TTarget2>> Page<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();


        T Scalar<T>(WhereExpression<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        T Scalar<T>(long id, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        T Scalar<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        T Scalar<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        T Scalar<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Tuple<TSource, TTarget> Scalar<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        Tuple<TSource, TTarget1, TTarget2> Scalar<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();



        DatabaseResult Add<T>(T item, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        DatabaseResult Delete<T>(T item, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        DatabaseResult Update<T>(T item, DbTransactionContext transContext = null) where T : DatabaseEntity, new();

        DatabaseResult BatchAdd<T>(IList<T> items, string lastUser, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        DatabaseResult BatchDelete<T>(IList<T> items, string lastUser, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        DatabaseResult BatchUpdate<T>(IList<T> items, string lastUser, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">as long as in the db that you want</typeparam>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        IDbTransaction CreateTransaction<T>(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : DatabaseEntity;

        SelectExpression<T> NewSelect<T>() where T : DatabaseEntity, new();
        FromExpression<T> NewFrom<T>() where T : DatabaseEntity, new();
        WhereExpression<T> NewWhere<T>() where T : DatabaseEntity, new();
    }
}