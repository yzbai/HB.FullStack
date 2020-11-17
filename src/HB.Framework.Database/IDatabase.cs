using HB.Framework.Common.Entities;
using HB.Framework.Database.Entities;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HB.Framework.Database
{
    public interface IDatabase
    {
        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new();

        /// <summary>
        /// Base on Guid字段
        /// </summary>
        Task AddOrUpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new();

        /// <summary>
        /// 返回每一个数据对应的Version
        /// </summary>
        Task<IEnumerable<Tuple<long, int>>> BatchAddOrUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transaction) where T : Entity, new();

        Task<IEnumerable<long>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new();

        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new();

        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new();

        Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : Entity, new();

        Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : Entity, new();

        Task<long> CountAsync<T>(SelectExpression<T>? selectCondition, FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : Entity, new();

        Task<long> CountAsync<T>(TransactionContext? transContext) where T : Entity, new();

        Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext) where T : Entity, new();

        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new();

        FromExpression<T> From<T>() where T : Entity, new();

        Task InitializeAsync(IEnumerable<Migration>? migrations = null);

        Task<IEnumerable<T>> PageAsync<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<T>> PageAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<T>> PageAsync<T>(long pageNumber, long perPageCount, TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<T>> PageAsync<T>(SelectExpression<T>? selectCondition, FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<T>> PageAsync<T>(WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext) where T : Entity, new();
        Task<IEnumerable<Tuple<TSource, TTarget?>>> PageAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget : Entity, new();
        Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> PageAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget1 : Entity, new()
            where TTarget2 : Entity, new();

        Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<T>> RetrieveAsync<T>(SelectExpression<T>? selectCondition, FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : Entity, new();

        Task<IEnumerable<TSelect>> RetrieveAsync<TSelect, TFrom, TWhere>(SelectExpression<TSelect>? selectCondition, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, TransactionContext? transContext = null)
            where TSelect : Entity, new()
            where TFrom : Entity, new()
            where TWhere : Entity, new();


        Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget : Entity, new();

        Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget1 : Entity, new()
            where TTarget2 : Entity, new();



        Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : Entity, new();

        Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : Entity, new();

        Task<T?> ScalarAsync<T>(long id, TransactionContext? transContext) where T : Entity, new();

        Task<T?> ScalarAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext? transContext) where T : Entity, new();

        Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : Entity, new();

        Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget : Entity, new();

        Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : Entity, new()
            where TTarget1 : Entity, new()
            where TTarget2 : Entity, new();

        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        SelectExpression<T> Select<T>() where T : Entity, new();

        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new();


        WhereExpression<T> Where<T>() where T : Entity, new();
    }
}