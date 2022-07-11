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
    public partial class DefaultDatabase : IDatabase
    {
        public async Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transactionContext = null) where T : ClientDatabaseModel, new()
        {
            DatabaseModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(modelDef.ModelFullName, modelDef.DatabaseName);
            }

            try
            {
                WhereExpression<T> where = Where(whereExpr).And(t => !t.Deleted);

                var command = DbCommandBuilder.CreateDeleteCommand(EngineType, modelDef, where);

                await _databaseEngine.ExecuteCommandNonQueryAsync(transactionContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, whereExpr.ToString(), ex);
            }
        }

        /// <summary>
        /// AddOrUpdate,即override,不检查Timestamp
        /// </summary>
        public async Task SetByIdAsync<T>(T item, /*string lastUser,*/ TransactionContext? transContext = null) where T : ClientDatabaseModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DatabaseModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(modelDef.ModelFullName, modelDef.DatabaseName);
            }

            //long oldTimestamp = -1;
            //string oldLastUser = "";

            try
            {
                ////Prepare
                //if (item is ServerDatabaseModel serverModel)
                //{
                //    oldTimestamp = serverModel.Timestamp;
                //    oldLastUser = serverModel.LastUser;

                //    serverModel.Timestamp = TimeUtil.UtcNowTicks;
                //    serverModel.LastUser = lastUser;
                //}

                var command = DbCommandBuilder.CreateAddOrUpdateCommand(EngineType, modelDef, item, false);

                _ = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task BatchAddOrUpdateByIdAsync<T>(IEnumerable<T> items, TransactionContext? transContext) where T : DatabaseModel, new()
        {
            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DatabaseModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.DatabaseNotWritable(modelDef.ModelFullName, SerializeUtil.ToJson(items));
            }

            try
            {
                var command = DbCommandBuilder.CreateBatchAddOrUpdateCommand(EngineType, modelDef, items, transContext == null);

                _ = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                string detail = $"Items:{SerializeUtil.ToJson(items)}";
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, detail, ex);
            }
        }
    }
}