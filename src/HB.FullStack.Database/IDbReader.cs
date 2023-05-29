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
        FromExpression<T> From<T>() where T : class, IDbModel;

        WhereExpression<T> Where<T>() where T : class, IDbModel;

        WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : class, IDbModel;

        WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IDbModel;

        Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : class, IDbModel;

        Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : class, IDbModel;

        Task<long> CountAsync<T>(TransactionContext? transContext) where T : class, IDbModel;

        Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext) where T : class, IDbModel;

        Task<IList<T>> RetrieveAllAsync<T>(TransactionContext? transContext, int? page = null, int? perPage = null, string? orderBy = null) where T : class, IDbModel;

        Task<IList<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext, int? page = null, int? perPage = null, string? orderBy = null)
            where T : class, IDbModel;

        Task<IList<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : class, IDbModel;

        Task<IList<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : class, IDbModel;

        Task<IList<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : class, IDbModel
            where TTarget : class, IDbModel;

        Task<IList<Tuple<TSource, TTarget1?, TTarget2?>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : class, IDbModel
            where TTarget1 : class, IDbModel
            where TTarget2 : class, IDbModel;

        Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : class, IDbModel;

        Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : class, IDbModel;

        Task<T?> ScalarAsync<T>(object id, TransactionContext? transContext) where T : class, IDbModel;

        Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext) where T : class, IDbModel;

        Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : class, IDbModel
            where TTarget : class, IDbModel;

        Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : class, IDbModel
            where TTarget1 : class, IDbModel
            where TTarget2 : class, IDbModel;

        //Task<IEnumerable<T>> RetrieveByForeignKeyAsync<T>(Expression<Func<T, object>> foreignKeyExp, object foreignKeyValue, TransactionContext? transactionContext, int? page, int? perPage, string? orderBy)
        //    where T : BaseDbModel;
    }
}