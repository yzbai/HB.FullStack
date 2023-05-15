using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.Config;
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
            bool? trulyDelete = null) where T : BaseDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
            //TruncateLastUser(ref lastUser);

            try
            {
                var command = DbCommandBuilder.CreateDeleteCommand(
                    modelDef,
                    id,
                    lastUser,
                    trulyDelete ?? _dbSchemaManager.GetDbSchema(modelDef.DbSchemaName).TrulyDelete,
                    oldTimestamp,
                    newTimestamp);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                long rows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(connectionString, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DbExceptions.ConcurrencyConflict(type: modelDef.FullName, id.ToString(), "");
                }
                else
                {
                    throw DbExceptions.FoundTooMuch(modelDef.FullName, item: id.ToString());
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.FullName, id.ToString(), ex);
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

        public Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull($"Lack ModelDef of {typeof(T).FullName}").ThrowIfNotWriteable();
            DbConflictCheckMethods bestConflictMethod = modelDef.BestConflictCheckMethodWhenDelete;


            bool trulyDelete = modelDef.DbSchema.TrulyDelete;
            object idValue = modelDef.PrimaryKeyPropertyDef.GetValueFrom(item)!;
            long curTimestamp = TimeUtil.Timestamp;

            DbEngineCommand command = bestConflictMethod switch
            {
                DbConflictCheckMethods.Ignore => DbCommandBuilder.CreateDeleteIgnoreConflictCheckCommand(modelDef, idValue, lastUser, trulyDelete, curTimestamp),
                DbConflictCheckMethods.OldNewValueCompare => DbCommandBuilder.CreateDeleteOldNewCompareCommand(modelDef, item, lastUser, trulyDelete, curTimestamp),
                DbConflictCheckMethods.Timestamp => DbCommandBuilder.CreateDeleteTimestampCommand(modelDef, idValue, (item as ITimestamp)!.Timestamp, lastUser, trulyDelete, curTimestamp),
                _ => throw DbExceptions.ConflictCheckError($"{modelDef.FullName} has wrong Best Conflict Check Method. {bestConflictMethod}")
            };

            ConnectionString connectionString = modelDef.DbSchema.ConnectionString!;

            long matched = transContext != null
                ? engine

        }

        private async Task BatchDeleteCoreAsync<T>(
            IList<object> ids,
            IList<long?> oldTimestamps,
            IList<long?> newTimestamps,
            string lastUser,
            TransactionContext? transContext,
            bool? trulyDelete = null) where T : DbModel, new()
        {
            if (ids.IsNullOrEmpty())
            {
                return;
            }


            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
            ThrowIfExceedMaxBatchNumber(ids, lastUser, modelDef);
            //TruncateLastUser(ref lastUser);

            try
            {
                var command = DbCommandBuilder.CreateBatchDeleteCommand(
                    modelDef,
                    ids,
                    oldTimestamps,
                    newTimestamps,
                    lastUser,
                    trulyDelete ?? _dbSchemaManager.GetDbSchema(modelDef.DbSchemaName).TrulyDelete,
                    transContext == null);

                var engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                using var reader = transContext != null
                    ? await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(connectionString, command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int affected = reader.GetInt32(0);

                    if (affected != 1)
                    {
                        throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(ids), $"not found the {count}th data item");
                    }

                    count++;
                }

                if (count != ids.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(ids), "");
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(ids), ex);
            }
        }
        public Task DeleteAsync<T>(IList<object> ids, TransactionContext? transContext, string lastUser, bool? trulyDelete = null) where T : TimelessDbModel, new()
        {
            long?[] oldTimestamps = new long?[ids.Count];
            long?[] newTimestamps = new long?[ids.Count];

            return BatchDeleteCoreAsync<T>(ids, oldTimestamps, newTimestamps, lastUser, transContext, trulyDelete);
        }

        public Task DeleteAsync<T>(IList<object> ids, IList<long?> timestamps, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : TimestampDbModel, new()
        {
            IList<long?> newTimestamps = Enumerable.Repeat<long?>(TimeUtil.Timestamp, ids.Count).ToList();

            return BatchDeleteCoreAsync<T>(ids, timestamps, newTimestamps, lastUser, transContext, trulyDelete);
        }

        public Task DeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : DbModel, new()
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
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);

            try
            {
                WhereExpression<T> where = Where(whereExpr).And(t => !t.Deleted);

                var command = DbCommandBuilder.CreateDeleteCommand(
                    modelDef,
                    where,
                    lastUser,
                    trulyDelete ?? _dbSchemaManager.GetDbSchema(modelDef.DbSchemaName).TrulyDelete);

                var engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                _ = transactionContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(connectionString, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, whereExpr.ToString(), ex);
            }
        }
    }
}
