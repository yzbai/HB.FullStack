using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.DBModels;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    public interface IDatabaseReader
    {

        FromExpression<T> From<T>() where T : DBModel, new();

        WhereExpression<T> Where<T>() where T : DBModel, new();

        WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DBModel, new();

        WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DBModel, new();

        Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DBModel, new();

        Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DBModel, new();

        Task<long> CountAsync<T>(TransactionContext? transContext) where T : DBModel, new();

        Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext) where T : DBModel, new();


        Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext, int? page = null, int? perPage = null, string? orderBy = null) where T : DBModel, new();
        Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext, int? page = null, int? perPage = null, string? orderBy = null) where T : DBModel, new();
        Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DBModel, new();

        Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DBModel, new();

        Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DBModel, new()
            where TTarget : DBModel, new();

        Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DBModel, new()
            where TTarget1 : DBModel, new()
            where TTarget2 : DBModel, new();

        Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DBModel, new();

        Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DBModel, new();

        Task<T?> ScalarAsync<T>(long id, TransactionContext? transContext) where T : DBModel, ILongIdModel, new();

        Task<T?> ScalarAsync<T>(Guid id, TransactionContext? transContext) where T : DBModel, IGuidIdModel, new();


        Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : DBModel, new();


        Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DBModel, new()
            where TTarget : DBModel, new();


        Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DBModel, new()
            where TTarget1 : DBModel, new()
            where TTarget2 : DBModel, new();

        Task<IEnumerable<T>> RetrieveByForeignKeyAsync<T>(Expression<Func<T, object>> foreignKeyExp, object foreignKeyValue, TransactionContext? transactionContext, int? page, int? perPage, string? orderBy)
            where T : DBModel, new();
    }
}