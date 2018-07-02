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
    public interface IDatabaseAsync
    {
		Task<IList<TSelect>> RetrieveAsync<TSelect, TFrom, TWhere>(Select<TSelect> selectCondition, From<TFrom> fromCondition, Where<TWhere> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
			where TSelect : DatabaseEntity, new()
			where TFrom : DatabaseEntity, new()
			where TWhere : DatabaseEntity, new();

        Task<IList<T>> RetrieveAsync<T>(Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<T>> RetrieveAsync<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<T>> RetrieveAsync<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<Tuple<TSource, TTarget>>> RetrieveAsync<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();

        Task<IList<Tuple<TSource, TTarget1, TTarget2>>> RetrieveAsync<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();

        Task<IList<T>> RetrieveAllAsync<T>(DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<long> CountAsync<T>(DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<long> CountAsync<T>(Where<T> condition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<long> CountAsync<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<long> CountAsync<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<T>> PageAsync<T>(long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<T>> PageAsync<T>(Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<T>> PageAsync<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<T>> PageAsync<T>(From<T> fromCondition, Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<T>> PageAsync<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<IList<Tuple<TSource, TTarget>>> PageAsync<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        Task<IList<Tuple<TSource, TTarget1, TTarget2>>> PageAsync<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();
        Task<T> ScalarAsync<T>(Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<T> ScalarAsync<T>(long id, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<T> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<T> ScalarAsync<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<T> ScalarAsync<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext = null, bool useMaster = false) where T : DatabaseEntity, new();
        Task<Tuple<TSource, TTarget>> ScalarAsync<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        Task<Tuple<TSource, TTarget1, TTarget2>> ScalarAsync<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();

        Task<DatabaseResult> AddAsync<T>(T item, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        Task<DatabaseResult> DeleteAsync<T>(T item, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        Task<DatabaseResult> UpdateAsync<T>(T item, DbTransactionContext transContext = null) where T : DatabaseEntity, new();

        Task<DatabaseResult> BatchAddAsync<T>(IList<T> items, string lastUser, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        Task<DatabaseResult> BatchDeleteAsync<T>(IList<T> items, string lastUser, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        Task<DatabaseResult> BatchUpdateAsync<T>(IList<T> items, string lastUser, DbTransactionContext transContext = null) where T : DatabaseEntity, new();
        
    }
}