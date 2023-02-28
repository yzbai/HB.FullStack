using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Extensions;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        private static void ThrowIfUpdateUsingTimestampNotValid(UpdateUsingTimestamp updatePack)
        {
            if (!updatePack.OldTimestamp.HasValue || updatePack.OldTimestamp.Value <= 638000651894004864)
            {
                throw DbExceptions.TimestampShouldBePositive(updatePack.OldTimestamp ?? 0);
            }

            if (updatePack.NewTimestamp.HasValue && updatePack.NewTimestamp.Value <= 638000651894004864)
            {
                throw DbExceptions.TimestampShouldBePositive(updatePack.NewTimestamp.Value);
            }

            if (updatePack.Id is long longId && longId <= 0)
            {
                throw DbExceptions.LongIdShouldBePositive(longId);
            }

            if (updatePack.Id is Guid guid && guid.IsEmpty())
            {
                throw DbExceptions.GuidShouldNotEmpty();
            }

            if (updatePack.PropertyNames.Count != updatePack.NewPropertyValues.Count)
            {
                throw DbExceptions.UpdateUsingTimestampListCountNotEqual();
            }

            if (updatePack.PropertyNames.Count <= 0)
            {
                throw DbExceptions.UpdateUsingTimestampListEmpty();
            }
        }

        private static void ThrowIfUpdateUsingCompareNotValid(UpdateUsingCompare updatePack)
        {
            if (updatePack.NewTimestamp.HasValue && updatePack.NewTimestamp.Value <= 638000651894004864)
            {
                throw DbExceptions.TimestampShouldBePositive(updatePack.NewTimestamp.Value);
            }

            if (updatePack.Id is long longId && longId <= 0)
            {
                throw DbExceptions.LongIdShouldBePositive(longId);
            }

            if (updatePack.Id is Guid guid && guid.IsEmpty())
            {
                throw DbExceptions.GuidShouldNotEmpty();
            }

            if (updatePack.PropertyNames.Count != updatePack.NewPropertyValues.Count || updatePack.OldPropertyValues.Count != updatePack.PropertyNames.Count)
            {
                throw DbExceptions.UpdateUsingTimestampListCountNotEqual();
            }

            if (updatePack.PropertyNames.Count <= 0)
            {
                throw DbExceptions.UpdateUsingTimestampListEmpty();
            }
        }

        public async Task UpdatePropertiesAsync<T>(UpdateUsingTimestamp updatePack, string lastUser, TransactionContext? transContext) where T : TimestampDbModel, new()
        {
            ThrowIfUpdateUsingTimestampNotValid(updatePack);

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);

            //TruncateLastUser(ref lastUser);

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesCommand(modelDef, updatePack, lastUser);
                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                long rows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(_dbSchemaManager.GetConnectionString(modelDef.DbSchemaName, true), command).ConfigureAwait(false);

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

        public async Task UpdatePropertiesAsync<T>(UpdateUsingCompare updatePack, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            ThrowIfUpdateUsingCompareNotValid(updatePack);

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);

            //TruncateLastUser(ref lastUser);

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesUsingCompareCommand(modelDef, updatePack, lastUser);
                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                int matchedRows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(_dbSchemaManager.GetConnectionString(modelDef.DbSchemaName, true), command).ConfigureAwait(false); ;

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

        public async Task UpdatePropertiesAsync<T>(PropertyChangePack changedPack, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            UpdateUsingCompare updatePack = changedPack.ToUpdateUsingCompare(modelDef);

            await UpdatePropertiesAsync<T>(updatePack, lastUser, transContext).ConfigureAwait(false);
        }

        public async Task BatchUpdatePropertiesAsync<T>(IList<UpdateUsingTimestamp> updatePacks, string lastUser, TransactionContext? transactionContext) where T : TimestampDbModel, new()
        {
            if (updatePacks.IsNullOrEmpty())
            {
                return;
            }

            if (updatePacks.Count == 1)
            {
                await UpdatePropertiesAsync<T>(updatePacks[0], lastUser, transactionContext).ConfigureAwait(false);
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);
            //TruncateLastUser(ref lastUser);

            //var updateChanges = ConvertToCommandTuple(updatePacks);

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesCommand(modelDef, updatePacks, lastUser, transactionContext == null);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                using IDataReader reader = transactionContext != null
                    ? await engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(_dbSchemaManager.GetConnectionString(modelDef.DbSchemaName, true), command).ConfigureAwait(false);

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

        public async Task BatchUpdatePropertiesAsync<T>(IList<UpdateUsingCompare> updatePacks, string lastUser, TransactionContext? transactionContext = null) where T : DbModel, new()
        {
            if (updatePacks.IsNullOrEmpty())
            {
                return;
            }

            if (updatePacks.Count == 1)
            {
                await UpdatePropertiesAsync<T>(updatePacks[0], lastUser, transactionContext);
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);
            //TruncateLastUser(ref lastUser);

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesUsingCompareCommand(modelDef, updatePacks, lastUser, transactionContext == null);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                using IDataReader reader = transactionContext != null
                    ? await engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(_dbSchemaManager.GetConnectionString(modelDef.DbSchemaName, true), command).ConfigureAwait(false); ;

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

        public Task BatchUpdatePropertiesAsync<T>(IEnumerable<PropertyChangePack> changedPacks, string lastUser, TransactionContext? transContext) where T : DbModel, new()
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

            return BatchUpdatePropertiesAsync<T>(changedPacks.Select(cp => cp.ToUpdateUsingCompare(modelDef)).ToList(), lastUser, transContext);
        }
    }
}
