using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;


using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    public interface IDatabaseReader
    {

        FromExpression<T> From<T>() where T : DatabaseEntity, new();

        WhereExpression<T> Where<T>() where T : DatabaseEntity, new();

        WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DatabaseEntity, new();

        WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DatabaseEntity, new();

        Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<long> CountAsync<T>(TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext) where T : DatabaseEntity, new();


        Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext, int? page, int? perPage, string? orderBy) where T : DatabaseEntity, new();
        Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext, int? page = null, int? perPage = null, string? orderBy = null) where T : DatabaseEntity, new();
        Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();

        Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();

        Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<T?> ScalarAsync<T>(long id, TransactionContext? transContext) where T : LongIdEntity, new();

        Task<T?> ScalarAsync<T>(Guid id, TransactionContext? transContext) where T : GuidEntity, new();


        Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DatabaseEntity, new();


        Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new();


        Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new();

        Task<IEnumerable<T>> RetrieveByForeignKeyAsync<T>(Expression<Func<T, object>> foreignKeyExp, object foreignKeyValue, TransactionContext? transactionContext, int? page, int? perPage, string? orderBy)
            where T : DatabaseEntity, new();
    }
}