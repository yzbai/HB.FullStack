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
            Select<TSelect> selectCondition, 
            From<TFrom> fromCondition, 
            Where<TWhere> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new();

        //delete
        IList<T> Retrieve<T>(
            Where<T> whereCondition, 
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
            From<T> fromCondition, 
            Where<T> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false) 
            where T : DatabaseEntity, new();

        //Delete
        IList<T> Retrieve<T>(
            Select<T> selectCondition, 
            From<T> fromCondition, 
            Where<T> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false) 
            where T : DatabaseEntity, new();


        IList<Tuple<TSource, TTarget>> Retrieve<TSource, TTarget>(
            From<TSource> fromCondition, 
            Where<TSource> whereCondition, 
            DbTransactionContext transContext = null, 
            bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();


        IList<Tuple<TSource, TTarget1, TTarget2>> Retrieve<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();


        IList<T> RetrieveAll<T>(DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();


        long Count<T>(DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        long Count<T>(Where<T> condition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        long Count<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        long Count<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        long Count<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();


        IList<T> Page<T>(long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<T> Page<T>(Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<T> Page<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<T> Page<T>(From<T> fromCondition, Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<T> Page<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        IList<Tuple<TSource, TTarget>> Page<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        IList<Tuple<TSource, TTarget1, TTarget2>> Page<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();


        T Scalar<T>(Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        T Scalar<T>(long id, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        T Scalar<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        T Scalar<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        T Scalar<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Tuple<TSource, TTarget> Scalar<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        Tuple<TSource, TTarget1, TTarget2> Scalar<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
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

        Select<T> Select<T>() where T : DatabaseEntity, new();
        From<T> From<T>() where T : DatabaseEntity, new();
        Where<T> Where<T>() where T : DatabaseEntity, new();
    }
}