using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        #region Timestamp

        public Task UpdatePropertiesAsync<T>(TimestampUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel, ITimestamp, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingTimestampAsync(modelDef, updatePack, lastUser, transContext);
        }

        public Task UpdatePropertiesAsync<T>(IList<TimestampUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext) where T : BaseDbModel, ITimestamp, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingTimestampAsync(modelDef, updatePacks, lastUser, transactionContext);
        }

        private async Task UpdatePropertiesUsingTimestampAsync(DbModelDef modelDef, TimestampUpdatePack updatePack, string lastUser, TransactionContext? transContext)
        {
            updatePack.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable().ThrowIfNotTimestamp();

            try
            {
                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);
                ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesTimestampCommand(modelDef, updatePack, lastUser);

                long rows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(connectionString, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, $"使用Timestamp版本的乐观锁，出现冲突。UpdatePack:{SerializeUtil.ToJson(updatePack)}, lastUser:{lastUser}", "");
                }
                else
                {
                    throw DbExceptions.FoundTooMuch(modelDef.FullName, $"UpdatePackUsingTimestamp:{updatePack}, lastUser:{lastUser}");
                }
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, $"UpdatePackUsingTimestamp: {updatePack} , lastUser: {lastUser}", ex);
            }
        }

        private async Task UpdatePropertiesUsingTimestampAsync(DbModelDef modelDef, IList<TimestampUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext)
        {
            if (updatePacks.IsNullOrEmpty())
            {
                return;
            }

            if (updatePacks.Count == 1)
            {
                await UpdatePropertiesUsingTimestampAsync(modelDef, updatePacks[0], lastUser, transactionContext).ConfigureAwait(false);
                return;
            }

            transactionContext.ThrowIfNull(nameof(transactionContext));
            updatePacks.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable().ThrowIfNotTimestamp();

            try
            {
                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);
                ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesTimestampCommand(modelDef, updatePacks, lastUser);

                using IDataReader reader = transactionContext != null
                    ? await engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(connectionString, command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "BatchUpdatePropertiesAsync");
                    }

                    count++;
                }

                if (count != updatePacks.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "BatchUpdatePropertiesAsync");
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region Timeless - using old new value compare

        public Task UpdatePropertiesAsync<T>(OldNewCompareUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : TimelessDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingOldNewCompareAsync(modelDef, updatePack, lastUser, transContext);
        }

        public Task UpdatePropertiesAsync<T>(IList<OldNewCompareUpdatePack> updatePacks, string lastUser, TransactionContext? transactionContext = null) where T : TimelessDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            return UpdatePropertiesUsingOldNewCompareAsync(modelDef, updatePacks, lastUser, transactionContext);
        }

        private async Task UpdatePropertiesUsingOldNewCompareAsync(DbModelDef modelDef, OldNewCompareUpdatePack updatePack, string lastUser, TransactionContext? transContext)
        {
            updatePack.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
            //TruncateLastUser(ref lastUser);

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesTimelessCommand(modelDef, updatePack, lastUser);
                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                int matchedRows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(connectionString, command).ConfigureAwait(false); ;

                if (matchedRows == 1)
                {
                    return;
                }
                else if (matchedRows == 0)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, $"使用新旧值对比的乐观锁出现冲突。UpdatePack:{updatePack}, lastUser:{lastUser}", "");
                }
                else
                {
                    throw DbExceptions.FoundTooMuch(modelDef.FullName, $"UpdatePack:{updatePack}, lastUser:{lastUser}");
                }
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, $"UpdatePack:{updatePack}, lastUser:{lastUser}", ex);
            }
        }

        private async Task UpdatePropertiesUsingOldNewCompareAsync(DbModelDef modelDef, IList<OldNewCompareUpdatePack> updatePacks, string lastUser, TransactionContext? transactionContext = null)
        {
            if (updatePacks.IsNullOrEmpty())
            {
                return;
            }

            if (updatePacks.Count == 1)
            {
                await UpdatePropertiesUsingOldNewCompareAsync(modelDef, updatePacks[0], lastUser, transactionContext);
                return;
            }

            modelDef.ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
            //TruncateLastUser(ref lastUser);

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesTimelessCommand(modelDef, updatePacks, lastUser, transactionContext == null);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                using IDataReader reader = transactionContext != null
                    ? await engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(connectionString, command).ConfigureAwait(false); ;

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "BatchUpdatePropertiesAsync");
                    }

                    count++;
                }

                if (count != updatePacks.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "BatchUpdatePropertiesAsync");
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region IgnoreConflictCheck

        public Task UpdatePropertiesAsync<T>()
        {

        }

        private Task UpdatePropertiesIgnoreConflictCheckAsync(DbModelDef modelDef, IgnoreConflictCheckUpdatePack ignorePack, string lastUser, TransactionContext? transContext)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region PropertyChangePack

        private static DbConflictCheckMethods GetPropertyChangePackConflictCheckMethod(DbModelDef modelDef, PropertyChangePack changePack)
        {
            if (modelDef.BestConflictCheckMethodWhenUpdateEntire == DbConflictCheckMethods.Timestamp && !changePack.ContainsProperty(nameof(ITimestamp.Timestamp)))
            {
                if (modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.OldNewValueCompare))
                {
                    return DbConflictCheckMethods.OldNewValueCompare;
                }
                else if (modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.Ignore))
                {
                    return DbConflictCheckMethods.Ignore;
                }
                else
                {
                    throw DbExceptions.ConflictCheckError($"Can not find proper conflict check method. For {modelDef.FullName}, changePack :{SerializeUtil.ToJson(changePack)}");
                }
            }

            return modelDef.BestConflictCheckMethodWhenUpdateEntire;
        }

        public async Task UpdatePropertiesAsync<T>(PropertyChangePack changedPack, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            DbConflictCheckMethods conflictCheckMethod = GetPropertyChangePackConflictCheckMethod(modelDef, changedPack);

            switch (conflictCheckMethod)
            {
                case DbConflictCheckMethods.Ignore:
                    IgnoreConflictCheckUpdatePack ignorePack = changedPack.ToIgnoreConflictCheckUpdatePack(modelDef);
                    await UpdatePropertiesIgnoreConflictCheckAsync(modelDef, ignorePack, lastUser, transContext).ConfigureAwait(false);
                    break;
                case DbConflictCheckMethods.OldNewValueCompare:
                    OldNewCompareUpdatePack pack = changedPack.ToOldNewCompareUpdatePack(modelDef);
                    await UpdatePropertiesUsingOldNewCompareAsync(modelDef, pack, lastUser, transContext).ConfigureAwait(false);
                    break;
                case DbConflictCheckMethods.Timestamp:
                    TimestampUpdatePack timestamPack = changedPack.ToTimestampUpdatePack(modelDef);
                    await UpdatePropertiesUsingTimestampAsync(modelDef, timestamPack, lastUser, transContext).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public Task UpdatePropertiesAsync<T>(IEnumerable<PropertyChangePack> changedPacks, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new()
        {
            if (changedPacks.IsNullOrEmpty())
            {
                return Task.CompletedTask;
            }

            if (changedPacks.Count() == 1)
            {
                return UpdatePropertiesAsync<T>(changedPacks.First(), lastUser, transContext);
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            //TODO: 是否允许不同
            DbConflictCheckMethods conflictCheckMethod = GetPropertyChangePackConflictCheckMethod(modelDef, changedPacks[0]);

            //if (modelDef.IsTimestamp)
            //{
            //    return UpdatePropertiesUsingTimestampAsync(modelDef, changedPacks.Select(cp => cp.ToTimestampUpdatePack(modelDef)).ToList(), lastUser, transContext);

            //}
            //else
            //{
            return UpdatePropertiesUsingOldNewCompareAsync(modelDef, changedPacks.Select(cp => cp.ToOldNewCompareUpdatePack(modelDef)).ToList(), lastUser, transContext);
            //}
        }

        #endregion
    }
}
