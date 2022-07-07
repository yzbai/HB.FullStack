/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Database.DatabaseModels;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    /// <summary>
    /// 只在客户端使用，因此不考虑并发，Version检测, 事务等等
    /// </summary>
    public static class DatabaseClientExtensions
    {
        public static async Task DeleteAsync<T>(this IDatabase database, Expression<Func<T, bool>> whereExpr, TransactionContext? transactionContext = null) where T : DatabaseModel, new()
        {
            DatabaseModelDef modelDef = database.ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(modelDef.ModelFullName, modelDef.DatabaseName);
            }

            try
            {
                WhereExpression<T> where = database.Where(whereExpr).And(t => !t.Deleted);

                var command = database.DbCommandBuilder.CreateDeleteCommand(database.EngineType, modelDef, where);

                await database.DatabaseEngine.ExecuteCommandNonQueryAsync(transactionContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, whereExpr.ToString(), ex);
            }
        }

        /// <summary>
        /// AddOrUpdate,即override，不改变version
        /// </summary>
        public static async Task SetByIdAsync<T>(this IDatabase database, T item, /*string lastUser, */TransactionContext? transContext = null) where T : DatabaseModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DatabaseModelDef modelDef = database.ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(modelDef.ModelFullName, modelDef.DatabaseName);
            }

            try
            {
                //DateTimeOffset utcNow = TimeUtil.UtcNow;
                //item.LastUser = lastUser;
                //item.LastTime = utcNow;
                //item.CreateTime = utcNow;

                if (item.Version < 0)
                {
                    item.Version = 0;
                }

                var command = database.DbCommandBuilder.CreateAddOrUpdateCommand(database.EngineType, modelDef, item);

                using var reader = await database.DatabaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, modelDef.DatabaseName!, command, true).ConfigureAwait(false);

                IList<T> models = reader.ToModels<T>(database.EngineType, database.ModelDefFactory, modelDef);

                T newItem = models[0];

                item.CreateTime = newItem.CreateTime;
                item.Version = newItem.Version;
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }
        }

        /// <summary>
        /// warning: 不改变items！！！！
        /// </summary>

        //public static async Task BatchAddOrUpdateByIdAsync<T>(this IDatabase database, IEnumerable<T> items, TransactionContext? transContext) where T : DatabaseModel, new()
        //{
        //    ThrowIf.NotValid(items, nameof(items));

        //    if (!items.Any())
        //    {
        //        return;
        //    }

        //    DatabaseModelDef modelDef = ModelDefFactory.GetDef<T>()!;

        //    if (!modelDef.DatabaseWriteable)
        //    {
        //        throw new DatabaseException(DatabaseErrorCode.DatabaseNotWriteable, $"Type:{modelDef.ModelFullName}, Items:{SerializeUtil.ToJson(items)}");
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

        //        var command = DbCommandBuilder.CreateBatchAddOrUpdateCommand(database.EngineType, modelDef, items, transContext == null);

        //        await database.DatabaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName, command).ConfigureAwait(false);
        //    }
        //    catch (Exception ex) when (!(ex is DatabaseException))
        //    {
        //        string detail = $"Items:{SerializeUtil.ToJson(items)}";
        //        throw new DatabaseException(DatabaseErrorCode.DatabaseError, $"Type:{modelDef.ModelFullName}, {detail}", ex);
        //    }
        //}
    }
}