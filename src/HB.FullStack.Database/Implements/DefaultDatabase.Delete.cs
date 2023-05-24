using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        public async Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull($"Lack ModelDef of {typeof(T).FullName}").ThrowIfNotWriteable();
            ConflictCheckMethods bestConflictMethod = modelDef.BestConflictCheckMethodWhenDelete;

            long curTimestamp = TimeUtil.Timestamp;
            bool trulyDelete = modelDef.DbSchema.TrulyDelete;

            try
            {
                DbEngineCommand command;

                switch (bestConflictMethod)
                {
                    case ConflictCheckMethods.Ignore:
                        object idValue = modelDef.PrimaryKeyPropertyDef.GetValueFrom(item)!;
                        command = DbCommandBuilder.CreateDeleteIgnoreConflictCheckCommand(modelDef, idValue, lastUser, trulyDelete, curTimestamp);
                        break;
                    case ConflictCheckMethods.OldNewValueCompare:
                        command = DbCommandBuilder.CreateDeleteOldNewCompareCommand(modelDef, item, lastUser, trulyDelete, curTimestamp);
                        break;
                    case ConflictCheckMethods.Timestamp:
                        object idValueTimestamp = modelDef.PrimaryKeyPropertyDef.GetValueFrom(item)!;
                        command = DbCommandBuilder.CreateDeleteTimestampCommand(modelDef, idValueTimestamp, (item as ITimestamp)!.Timestamp, lastUser, trulyDelete, curTimestamp);
                        break;
                    default:
                        throw DbExceptions.ConflictCheckError($"{modelDef.FullName} has wrong Best Conflict Check Method. {bestConflictMethod}");
                }

                long rows = transContext != null
                        ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                        : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatch(modelDef, rows, item, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task DeleteAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel
        {
            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            if (items.Count == 1)
            {
                await DeleteAsync(items[0], lastUser, transContext).ConfigureAwait(false);
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull($"Lack ModelDef of {typeof(T).FullName}").ThrowIfNotWriteable();
            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);

            ConflictCheckMethods bestConflictMethod = modelDef.BestConflictCheckMethodWhenDelete;

            long curTimestamp = TimeUtil.Timestamp;
            bool trulyDelete = modelDef.DbSchema.TrulyDelete;

            try
            {
                DbEngineCommand command;

                switch (bestConflictMethod)
                {
                    case ConflictCheckMethods.Ignore:
                        var idValues = items.Select(item => modelDef.PrimaryKeyPropertyDef.GetValueFrom(item)!).ToList();
                        command = DbCommandBuilder.CreateBatchDeleteIgnoreConflictCheckCommand(modelDef, idValues, lastUser, trulyDelete, curTimestamp);
                        break;
                    case ConflictCheckMethods.OldNewValueCompare:
                        command = DbCommandBuilder.CreateBatchDeleteOldNewCompareCommand(modelDef, items, lastUser, trulyDelete, curTimestamp);
                        break;
                    case ConflictCheckMethods.Timestamp:
                        var idValues2 = items.Select(item => modelDef.PrimaryKeyPropertyDef.GetValueFrom(item)!).ToList();
                        var timestamps = items.Select(item => (item as ITimestamp)!.Timestamp).ToList();
                        command = DbCommandBuilder.CreateBatchDeleteTimestampCommand(modelDef, idValues2, timestamps, lastUser, trulyDelete, curTimestamp);
                        break;
                    default:
                        throw DbExceptions.ConflictCheckError($"{modelDef.FullName} has wrong Best Conflict Check Method. {bestConflictMethod}");
                }

                using var reader = transContext != null
                        ? await modelDef.Engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                        : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatches(modelDef, reader, items, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(items), ex);
            }
        }

        public async Task DeleteAsync<T>(object id, long timestamp, string lastUser, TransactionContext? transContext) where T : BaseDbModel, ITimestamp
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull($"Lack ModelDef of {typeof(T).FullName}").ThrowIfNotWriteable();

            long curTimestamp = TimeUtil.Timestamp;
            bool trulyDelete = modelDef.DbSchema.TrulyDelete;

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateDeleteTimestampCommand(modelDef, id, timestamp, lastUser, trulyDelete, curTimestamp);

                long rows = transContext != null
                        ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                        : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatch(modelDef, rows, id, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(id), ex);
            }
        }

        public async Task DeleteAsync<T>(IList<object> ids, IList<long> timestamps, string lastUser, TransactionContext transContext) where T : BaseDbModel, ITimestamp
        {
            if (!ids.Any())
            {
                return;
            }

            ThrowIf.CountNotEqual(ids, timestamps, "not even");

            if (ids.Count == 1)
            {
                await DeleteAsync<T>(ids[0], timestamps[0], lastUser, transContext).ConfigureAwait(false);
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull($"Lack ModelDef of {typeof(T).FullName}").ThrowIfNotWriteable();
            ThrowIfExceedMaxBatchNumber(ids, lastUser, modelDef);

            long curTimestamp = TimeUtil.Timestamp;
            bool trulyDelete = modelDef.DbSchema.TrulyDelete;

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchDeleteTimestampCommand(modelDef, ids, timestamps, lastUser, trulyDelete, curTimestamp);

                using var reader = transContext != null
                        ? await modelDef.Engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                        : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatches(modelDef, reader, ids, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(ids), ex);
            }
        }

        public async Task DeleteIgnoreConflictCheckAsync<T>(object id, string lastUser, TransactionContext? transContext) where T : BaseDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull($"Lack ModelDef of {typeof(T).FullName}").ThrowIfNotWriteable();

            long curTimestamp = TimeUtil.Timestamp;
            bool trulyDelete = modelDef.DbSchema.TrulyDelete;

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateDeleteIgnoreConflictCheckCommand(modelDef, id, lastUser, trulyDelete, curTimestamp);

                long rows = transContext != null
                        ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                        : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatch(modelDef, rows, id, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(id), ex);
            }
        }

        public async Task DeleteIgnoreConflictCheckAsync<T>(IList<object> ids, string lastUser, TransactionContext transContext) where T : BaseDbModel
        {
            if (!ids.Any())
            {
                return;
            }

            if (ids.Count == 1)
            {
                await DeleteIgnoreConflictCheckAsync<T>(ids[0], lastUser, transContext).ConfigureAwait(false);
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull($"Lack ModelDef of {typeof(T).FullName}").ThrowIfNotWriteable();
            ThrowIfExceedMaxBatchNumber(ids, lastUser, modelDef);

            long curTimestamp = TimeUtil.Timestamp;
            bool trulyDelete = modelDef.DbSchema.TrulyDelete;

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchDeleteIgnoreConflictCheckCommand(modelDef, ids, lastUser, trulyDelete, curTimestamp);

                using var reader = transContext != null
                        ? await modelDef.Engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                        : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatches(modelDef, reader, ids, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(ids), ex);
            }
        }

        public async Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, string lastUser, TransactionContext transactionContext) where T : BaseDbModel
        {
            //TODO: 这里应该添加安全限制，检查whereExpr, 或者先select，然后判断是否删除,记录删除日志
            //ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull($"Lack ModelDef of {typeof(T).FullName}").ThrowIfNotWriteable();

            try
            {
                WhereExpression<T> whereCondition = Where(whereExpr);

                DbEngineCommand command = DbCommandBuilder.CreateDeleteConditonCommand<T>(modelDef, whereCondition, lastUser, modelDef.DbSchema.TrulyDelete);

                _ = transactionContext != null
                        ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                        : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, whereExpr.ToString(), ex);
            }
        }
    }
}
