using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        private async Task DeleteCoreAsync<T>(
            object id,
            long? oldTimestamp,
            long? newTimestamp,
            string lastUser,
            TransactionContext? transContext,
            bool? trulyDelete = null) where T : DbModel, new()
        {

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);
            TruncateLastUser(ref lastUser);

            try
            {
                var command = DbCommandBuilder.CreateDeleteCommand(
                    modelDef,
                    id,
                    lastUser,
                    trulyDelete ?? DbSettingManager.GetDefaultTrulyDelete(modelDef.DbSchema),
                    oldTimestamp,
                    newTimestamp);

                IDatabaseEngine engine = DbSettingManager.GetDatabaseEngine(modelDef.EngineType);

                long rows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(DbSettingManager.GetConnectionString(modelDef.DbSchema, true), command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(type: modelDef.ModelFullName, id.ToString(), "");
                }
                else
                {
                    throw DatabaseExceptions.FoundTooMuch(modelDef.ModelFullName, item: id.ToString());
                }
            }
            catch (Exception ex)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, id.ToString(), ex);
            }
        }

        public Task DeleteAsync<T>(object id, long timestamp, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : TimestampDbModel, new()
        {
            return DeleteCoreAsync<T>(id, timestamp, TimeUtil.Timestamp, lastUser, transContext, trulyDelete);
        }

        public Task DeleteAsync<T>(object id, TransactionContext? transContext, string lastUser, bool? trulyDelete = null) where T : TimelessDbModel, new()
        {
            return DeleteCoreAsync<T>(id, null, null, lastUser, transContext, trulyDelete);
        }

        public Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : DbModel, new()
        {
            object id = item is ILongId longIdItem ? longIdItem.Id : ((IGuidId)item).Id;

            if (item is TimestampDbModel tModel)
            {
                return DeleteCoreAsync<T>(id, tModel.Timestamp, TimeUtil.Timestamp, lastUser, transContext, trulyDelete);

            }
            else
            {
                return DeleteCoreAsync<T>(id, null, null, lastUser, transContext, trulyDelete);
            }
        }

        private async Task BatchDeleteCoreAsync<T>(
            IList<object> ids,
            IList<long?> oldTimestamps,
            IList<long?> newTimestamps,
            string lastUser,
            TransactionContext? transContext,
            bool? trulyDeleted = null) where T : DbModel, new()
        {
            if (ids.IsNullOrEmpty())
            {
                return;
            }


            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);
            ThrowIfTooMuchItems(ids, lastUser, modelDef);
            TruncateLastUser(ref lastUser);

            try
            {
                var command = DbCommandBuilder.CreateBatchDeleteCommand(
                    modelDef,
                    ids,
                    oldTimestamps,
                    newTimestamps,
                    lastUser,
                    trulyDeleted ?? DbSettingManager.GetDefaultTrulyDelete(modelDef.DbSchema),
                    transContext == null);

                var engine = DbSettingManager.GetDatabaseEngine(modelDef.EngineType);

                using var reader = transContext != null
                    ? await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(DbSettingManager.GetConnectionString(modelDef.DbSchema, true), command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int affected = reader.GetInt32(0);

                    if (affected != 1)
                    {
                        throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(ids), $"not found the {count}th data item");
                    }

                    count++;
                }

                if (count != ids.Count)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(ids), "");
                }
            }
            catch (Exception ex)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(ids), ex);
            }
        }
        public Task BatchDeleteAsync<T>(IList<object> ids, TransactionContext? transContext, string lastUser, bool? trulyDelete = null) where T : TimelessDbModel, new()
        {
            long?[] oldTimestamps = new long?[ids.Count];
            long?[] newTimestamps = new long?[ids.Count];

            return BatchDeleteCoreAsync<T>(ids, oldTimestamps, newTimestamps, lastUser, transContext, trulyDelete);
        }

        public Task BatchDeleteAsync<T>(IList<object> ids, IList<long?> timestamps, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : TimestampDbModel, new()
        {
            IList<long?> newTimestamps = Enumerable.Repeat<long?>(TimeUtil.Timestamp, ids.Count).ToList();

            return BatchDeleteCoreAsync<T>(ids, timestamps, newTimestamps, lastUser, transContext, trulyDelete);
        }

        public Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : DbModel, new()
        {
            IList<object> ids = items is IEnumerable<ILongId> longIds
                ? longIds.Select<ILongId, object>(i => i.Id).ToList()
                : ((IEnumerable<IGuidId>)items).Select<IGuidId, object>(i => i.Id).ToList();

            if (items is IEnumerable<TimestampDbModel> tModels)
            {
                IList<long?> oldTimestamps = tModels.Select<TimestampDbModel, long?>(i => i.Timestamp).ToList();
                IList<long?> newTimestamps = Enumerable.Repeat<long?>(TimeUtil.Timestamp, ids.Count).ToList();

                return BatchDeleteCoreAsync<T>(ids, oldTimestamps, newTimestamps, lastUser, transContext, trulyDelete);
            }
            else
            {
                long?[] oldTimestamps = new long?[ids.Count];
                long?[] newTimestamps = new long?[ids.Count];

                return BatchDeleteCoreAsync<T>(ids, oldTimestamps, newTimestamps, lastUser, transContext, trulyDelete);
            }
        }

        public async Task DeleteAsync<T>(
            Expression<Func<T, bool>> whereExpr,
            string lastUser,
            TransactionContext? transactionContext = null,
            bool? trulyDelete = null) where T : TimelessDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);

            try
            {
                WhereExpression<T> where = Where(whereExpr).And(t => !t.Deleted);

                var command = DbCommandBuilder.CreateDeleteCommand(
                    modelDef,
                    where,
                    lastUser,
                    trulyDelete ?? DbSettingManager.GetDefaultTrulyDelete(modelDef.DbSchema));

                var engine = DbSettingManager.GetDatabaseEngine(modelDef.EngineType);

                _ = transactionContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(DbSettingManager.GetConnectionString(modelDef.DbSchema, true), command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, whereExpr.ToString(), ex);
            }
        }
    }
}
