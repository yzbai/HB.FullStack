using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    public interface IDbReader
    {
        FromExpression<T> From<T>() where T : BaseDbModel;

        WhereExpression<T> Where<T>() where T : BaseDbModel;

        WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : BaseDbModel;

        WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : BaseDbModel;

        Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : BaseDbModel;

        Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : BaseDbModel;

        Task<long> CountAsync<T>(TransactionContext? transContext) where T : BaseDbModel;

        Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext) where T : BaseDbModel;

        Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext, int? page = null, int? perPage = null, string? orderBy = null) where T : BaseDbModel;

        Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext, int? page = null, int? perPage = null, string? orderBy = null) where T : BaseDbModel;

        Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : BaseDbModel;

        Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : BaseDbModel;

        Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : BaseDbModel
            where TTarget : BaseDbModel;

        Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : BaseDbModel
            where TTarget1 : BaseDbModel
            where TTarget2 : BaseDbModel;

        Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : BaseDbModel;

        Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : BaseDbModel;

        Task<T?> ScalarAsync<T>(object id, TransactionContext? transContext) where T : BaseDbModel;

        Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : BaseDbModel;

        Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : BaseDbModel
            where TTarget : BaseDbModel;

        Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : BaseDbModel
            where TTarget1 : BaseDbModel
            where TTarget2 : BaseDbModel;

        //Task<IEnumerable<T>> RetrieveByForeignKeyAsync<T>(Expression<Func<T, object>> foreignKeyExp, object foreignKeyValue, TransactionContext? transactionContext, int? page, int? perPage, string? orderBy)
        //    where T : BaseDbModel;
    }
}