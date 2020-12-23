using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    /// <summary>
    /// 只在客户端使用，因此不考虑并发，Version检测, 事务等等
    /// </summary>
    public static class DatabaseClientExtensions
    {
        public static async Task DeleteAsync<T>(this IDatabase database, Expression<Func<T, bool>> whereExpr, TransactionContext? transactionContext = null) where T : DatabaseEntity2, new()
        {
            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotWriteable, entityDef.EntityFullName);
            }

            try
            {
                WhereExpression<T> where = database.Where(whereExpr).And(t => !t.Deleted);

                var command = DbCommandBuilder.CreateDeleteCommand(database.EngineType, entityDef, where);

                await database.DatabaseEngine.ExecuteCommandNonQueryAsync(transactionContext?.Transaction, entityDef.DatabaseName!, command).ConfigureAwait(false);

            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                throw new DatabaseException(ErrorCode.DatabaseError, entityDef.EntityFullName, "", ex); ;
            }
        }

        private static TransactionContext GetFakeTransactionContext()
        {
            return new TransactionContext(null!, TransactionStatus.InTransaction, null!);
        }

        public static Task UpdateAsync<T>(this IDatabase database, IEnumerable<T> items, TransactionContext? transContext = null) where T : DatabaseEntity2, new()
        {
            TransactionContext context = transContext ?? GetFakeTransactionContext();

            return database.BatchUpdateAsync(items, "", context);
        }

        public static Task DeleteAsync<T>(this IDatabase database, IEnumerable<T> items, TransactionContext? transContext = null) where T : DatabaseEntity2, new()
        {
            TransactionContext context = transContext ?? GetFakeTransactionContext();

            return database.BatchDeleteAsync(items, "", context);
        }

        public static Task<IEnumerable<long>> AddAsync<T>(this IDatabase database, IEnumerable<T> items, TransactionContext? transContext = null) where T : DatabaseEntity2, new()
        {
            TransactionContext context = transContext ?? GetFakeTransactionContext();

            return database.BatchAddAsync<T>(items, "", context);
        }

        public static async Task SetAsync<T>(this IDatabase database, Expression<Func<T, bool>> whereExpr, IEnumerable<T> newItems, TransactionContext? transContext = null) where T : DatabaseEntity2, new()
        {
            TransactionContext context = transContext ?? GetFakeTransactionContext();

            await database.DeleteAsync<T>(whereExpr, context).ConfigureAwait(false);

            await database.AddAsync<T>(newItems, context);
        }
    }
}
