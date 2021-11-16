using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    /// <summary>
    /// 只在客户端使用，因此不考虑并发，Version检测, 事务等等
    /// </summary>
    public static class DatabaseClientExtensions
    {
        public static async Task DeleteAsync<T>(this IDatabase database, Expression<Func<T, bool>> whereExpr, TransactionContext? transactionContext = null) where T : DatabaseEntity, new()
        {
            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(entityDef.EntityFullName, entityDef.DatabaseName);
            }

            try
            {
                WhereExpression<T> where = database.Where(whereExpr).And(t => !t.Deleted);

                var command = DbCommandBuilder.CreateDeleteCommand(database.EngineType, entityDef, where);

                await database.DatabaseEngine.ExecuteCommandNonQueryAsync(transactionContext?.Transaction, entityDef.DatabaseName!, command).ConfigureAwait(false);

            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(entityDef.EntityFullName, whereExpr.ToString(), ex);
            }
        }

        public static async Task AddOrUpdateByIdAsync<T>(this IDatabase database, T item, string lastUser, TransactionContext? transContext = null) where T : DatabaseEntity, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

            if (!entityDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(entityDef.EntityFullName, entityDef.DatabaseName);
            }

            try
            {
                item.LastUser = lastUser;
                item.LastTime = TimeUtil.UtcNow;

                if (item.Version < 0)
                {
                    item.Version = 0;
                }

                var command = DbCommandBuilder.CreateAddOrUpdateCommand(database.EngineType, entityDef, item);

                using var reader = await database.DatabaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName!, command, true).ConfigureAwait(false);

                IList<T> entities = reader.ToEntities<T>(database.EngineType, entityDef);

                T newItem = entities[0];

                item.CreateTime = newItem.CreateTime;
                item.Version = newItem.Version;
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(entityDef.EntityFullName, SerializeUtil.ToJson(item), ex);
            }
        }

        /// <summary>
        /// warning: 不改变items！！！！
        /// </summary>
        /// <exception cref="DatabaseException"></exception>
        //public static async Task BatchAddOrUpdateByIdAsync<T>(this IDatabase database, IEnumerable<T> items, TransactionContext? transContext) where T : DatabaseEntity, new()
        //{
        //    ThrowIf.NotValid(items, nameof(items));

        //    if (!items.Any())
        //    {
        //        return;
        //    }

        //    EntityDef entityDef = EntityDefFactory.GetDef<T>()!;

        //    if (!entityDef.DatabaseWriteable)
        //    {
        //        throw new DatabaseException(DatabaseErrorCode.DatabaseNotWriteable, $"Type:{entityDef.EntityFullName}, Items:{SerializeUtil.ToJson(items)}");
        //    }

        //    try
        //    {
        //        foreach (var item in items)
        //        {
        //            item.LastTime = TimeUtil.UtcNow;

        //            if (item.Version < 0)
        //            {
        //                item.Version = 0;
        //            }
        //        }

        //        var command = DbCommandBuilder.CreateBatchAddOrUpdateCommand(database.EngineType, entityDef, items, transContext == null);

        //        await database.DatabaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName, command).ConfigureAwait(false);
        //    }
        //    catch (Exception ex) when (!(ex is DatabaseException))
        //    {
        //        string detail = $"Items:{SerializeUtil.ToJson(items)}";
        //        throw new DatabaseException(DatabaseErrorCode.DatabaseError, $"Type:{entityDef.EntityFullName}, {detail}", ex);
        //    }
        //}

    }
}
