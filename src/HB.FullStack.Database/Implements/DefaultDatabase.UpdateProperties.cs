using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        #region Timestamp

        public Task UpdatePropertiesAsync<T>(TimestampUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : TimestampDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingTimestampAsync(modelDef, updatePack, lastUser, transContext);
        }

        public Task UpdatePropertiesAsync<T>(IList<TimestampUpdatePack> updatePacks, string lastUser, TransactionContext? transactionContext) where T : TimestampDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingTimestampAsync(modelDef, updatePacks, lastUser, transactionContext);
        }

        private async Task UpdatePropertiesUsingTimestampAsync(DbModelDef modelDef, TimestampUpdatePack updatePack, string lastUser, TransactionContext? transContext)
        {
            updatePack.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
            //TruncateLastUser(ref lastUser);

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesTimestampCommand(modelDef, updatePack, lastUser);
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
                    throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, $"使用Timestamp版本的乐观锁，出现冲突。UpdatePack:{SerializeUtil.ToJson(updatePack)}, lastUser:{lastUser}", "");
                }
                else
                {
                    throw DbExceptions.FoundTooMuch(modelDef.ModelFullName, $"UpdatePackUsingTimestamp:{updatePack}, lastUser:{lastUser}");
                }
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.ModelFullName, $"UpdatePackUsingTimestamp: {updatePack} , lastUser: {lastUser}", ex);
            }
        }

        private async Task UpdatePropertiesUsingTimestampAsync(DbModelDef modelDef, IList<TimestampUpdatePack> updatePacks, string lastUser, TransactionContext? transactionContext)
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

            modelDef.ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
            //TruncateLastUser(ref lastUser);

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesTimestampCommand(modelDef, updatePacks, lastUser, transactionContext == null);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                using IDataReader reader = transactionContext != null
                    ? await engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(connectionString, command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(updatePacks), "BatchUpdatePropertiesAsync");
                    }

                    count++;
                }

                if (count != updatePacks.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(updatePacks), "BatchUpdatePropertiesAsync");
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(updatePacks), ex);
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
                    throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, $"使用新旧值对比的乐观锁出现冲突。UpdatePack:{updatePack}, lastUser:{lastUser}", "");
                }
                else
                {
                    throw DbExceptions.FoundTooMuch(modelDef.ModelFullName, $"UpdatePack:{updatePack}, lastUser:{lastUser}");
                }
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.ModelFullName, $"UpdatePack:{updatePack}, lastUser:{lastUser}", ex);
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
                        throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(updatePacks), "BatchUpdatePropertiesAsync");
                    }

                    count++;
                }

                if (count != updatePacks.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(updatePacks), "BatchUpdatePropertiesAsync");
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region PropertyChangePack - Timeless

        public async Task UpdatePropertiesAsync<T>(PropertyChangePack changedPack, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            //if (modelDef.IsTimestamp)
            //{
            //    TimestampUpdatePack updatePack = changedPack.ToTimestampUpdatePack(modelDef);

            //    await UpdatePropertiesUsingTimestampAsync(modelDef, updatePack, lastUser, transContext).ConfigureAwait(false);
            //}
            //else
            //{
            OldNewCompareUpdatePack updatePack = changedPack.ToOldNewCompareUpdatePack(modelDef);

            await UpdatePropertiesUsingOldNewCompareAsync(modelDef, updatePack, lastUser, transContext).ConfigureAwait(false);
            //}
        }

        public Task UpdatePropertiesAsync<T>(IEnumerable<PropertyChangePack> changedPacks, string lastUser, TransactionContext? transContext) where T : DbModel, new()
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

        #region Update Properties without any conflict check

        //TODO: Update Properties without any conflict check

        #endregion
    }
}
