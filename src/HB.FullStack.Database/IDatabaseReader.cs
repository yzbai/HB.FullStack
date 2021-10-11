﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;


using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    public interface IDatabaseReader
    {
        /// <exception cref="DatabaseException"></exception>
        FromExpression<T> From<T>() where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        WhereExpression<T> Where<T>() where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<long> CountAsync<T>(TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<T>> PageAsync<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<T>> PageAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<T>> PageAsync<T>(long pageNumber, long perPageCount, TransactionContext? transContext) where T : DatabaseEntity, new();

        //Task<IEnumerable<T>> PageAsync<T>(SelectExpression<T>? selectCondition, FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext) where T : DatabaseEntity2, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<T>> PageAsync<T>(WhereExpression<T>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<Tuple<TSource, TTarget?>>> PageAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> PageAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, long pageNumber, long perPageCount, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DatabaseEntity, new();
        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<T?> ScalarAsync<T>(long id, TransactionContext? transContext) where T : LongIdEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<T?> ScalarAsync<T>(Guid id, TransactionContext? transContext) where T : GuidEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();
    }
}