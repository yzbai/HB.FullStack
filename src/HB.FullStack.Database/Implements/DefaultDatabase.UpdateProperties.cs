/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    internal partial class DefaultDatabase
    {
        #region Timestamp

        public Task UpdatePropertiesAsync<T>(TimestampUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingTimestampAsync(modelDef, updatePack, lastUser, transContext);
        }

        public Task UpdatePropertiesAsync<T>(IList<TimestampUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext) where T : BaseDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingTimestampAsync(modelDef, updatePacks, lastUser, transactionContext);
        }

        private async Task UpdatePropertiesUsingTimestampAsync(DbModelDef modelDef, TimestampUpdatePack updatePack, string lastUser, TransactionContext? transContext)
        {
            updatePack.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable().ThrowIfNotTimestamp();

            if (!modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Timestamp))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow Timestamp Conflict Check.");
            }

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesTimestampCommand(modelDef, updatePack, lastUser);

                long rows = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);
                
                CheckFoundMatch(modelDef, rows, updatePack, lastUser);
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

            ThrowIfExceedMaxBatchNumber(updatePacks, lastUser, modelDef);

            transactionContext.ThrowIfNull(nameof(transactionContext));
            updatePacks.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable().ThrowIfNotTimestamp();

            if (!modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Timestamp))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow Timestamp Conflict Check.");
            }

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesTimestampCommand(modelDef, updatePacks, lastUser);

                using IDataReader reader = transactionContext != null
                    ? await modelDef.Engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatches(modelDef, reader, updatePacks, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region OldNewCompare

        public Task UpdatePropertiesAsync<T>(OldNewCompareUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingOldNewCompareAsync(modelDef, updatePack, lastUser, transContext);
        }

        public Task UpdatePropertiesAsync<T>(IList<OldNewCompareUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext) where T : BaseDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            return UpdatePropertiesUsingOldNewCompareAsync(modelDef, updatePacks, lastUser, transactionContext);
        }

        private async Task UpdatePropertiesUsingOldNewCompareAsync(DbModelDef modelDef, OldNewCompareUpdatePack updatePack, string lastUser, TransactionContext? transContext)
        {
            updatePack.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable();

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesOldNewCompareCommand(modelDef, updatePack, lastUser);

                int matchedRows = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false); ;

                CheckFoundMatch(modelDef, matchedRows, updatePack, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, $"UpdatePack:{updatePack}, lastUser:{lastUser}", ex);
            }
        }

        private async Task UpdatePropertiesUsingOldNewCompareAsync(DbModelDef modelDef, IList<OldNewCompareUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext)
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

            ThrowIfExceedMaxBatchNumber(updatePacks, lastUser, modelDef);

            transactionContext.ThrowIfNull(nameof(transactionContext));
            modelDef.ThrowIfNotWriteable();

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesOldNewCompareCommand(modelDef, updatePacks, lastUser);

                using IDataReader reader = transactionContext != null
                    ? await modelDef.Engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false) 
                    : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatches(modelDef, reader, updatePacks, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region IgnoreConflictCheck

        public Task UpdatePropertiesAsync<T>(IgnoreConflictCheckUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesIgnoreConflictCheckAsync(modelDef, updatePack, lastUser, transContext);
        }

        public Task UpdatePropertiesAsync<T>(IList<IgnoreConflictCheckUpdatePack> updatePacks, string lastUser, TransactionContext transContext) where T : BaseDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesIgnoreConflictCheckAsync(modelDef, updatePacks, lastUser, transContext);
        }

        private async Task UpdatePropertiesIgnoreConflictCheckAsync(DbModelDef modelDef, IgnoreConflictCheckUpdatePack updatePack, string lastUser, TransactionContext? transContext)
        {
            updatePack.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable();

            if (!modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Ignore))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow Ignore Conflict Check.");
            }

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesIgnoreConflictCheckCommand(modelDef, updatePack, lastUser);

                long rows = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false) 
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatch(modelDef, rows, updatePack, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, $"UpdatePackUsingTimestamp: {updatePack} , lastUser: {lastUser}", ex);
            }
        }

        private async Task UpdatePropertiesIgnoreConflictCheckAsync(DbModelDef modelDef, IList<IgnoreConflictCheckUpdatePack> updatePacks, string lastUser, TransactionContext? transactionContext)
        {
            if (updatePacks.IsNullOrEmpty())
            {
                return;
            }

            if (updatePacks.Count == 1)
            {
                await UpdatePropertiesIgnoreConflictCheckAsync(modelDef, updatePacks[0], lastUser, transactionContext).ConfigureAwait(false);
                return;
            }

            ThrowIfExceedMaxBatchNumber(updatePacks, lastUser, modelDef);

            transactionContext.ThrowIfNull(nameof(transactionContext));
            updatePacks.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable();

            if (!modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Ignore))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow Ignore Conflict Check.");
            }

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesIgnoreConflictCheckCommand(modelDef, updatePacks, lastUser);

                using IDataReader reader = transactionContext != null
                    ? await modelDef.Engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false) 
                    : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatches(modelDef, reader, updatePacks, lastUser);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region PropertyChangePack

        private static ConflictCheckMethods GetPropertyChangePackConflictCheckMethod(DbModelDef modelDef, PropertyChangePack changePack)
        {
            if (modelDef.BestConflictCheckMethodWhenUpdate == ConflictCheckMethods.Timestamp && !changePack.ContainsProperty(nameof(ITimestamp.Timestamp)))
            {
                if (modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.OldNewValueCompare))
                {
                    return ConflictCheckMethods.OldNewValueCompare;
                }
                else if (modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Ignore))
                {
                    return ConflictCheckMethods.Ignore;
                }
                else
                {
                    throw DbExceptions.ConflictCheckError($"Can not find proper conflict check method. For {modelDef.FullName}, changePack :{SerializeUtil.ToJson(changePack)}");
                }
            }

            return modelDef.BestConflictCheckMethodWhenUpdate;
        }

        public async Task UpdatePropertiesAsync<T>(PropertyChangePack changedPack, string lastUser, TransactionContext? transContext) where T : BaseDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ConflictCheckMethods conflictCheckMethod = GetPropertyChangePackConflictCheckMethod(modelDef, changedPack);

            switch (conflictCheckMethod)
            {
                case ConflictCheckMethods.Ignore:
                    IgnoreConflictCheckUpdatePack ignorePack = changedPack.ToIgnoreConflictCheckUpdatePack(modelDef);
                    await UpdatePropertiesIgnoreConflictCheckAsync(modelDef, ignorePack, lastUser, transContext).ConfigureAwait(false);
                    break;

                case ConflictCheckMethods.OldNewValueCompare:
                    OldNewCompareUpdatePack pack = changedPack.ToOldNewCompareUpdatePack(modelDef);
                    await UpdatePropertiesUsingOldNewCompareAsync(modelDef, pack, lastUser, transContext).ConfigureAwait(false);
                    break;

                case ConflictCheckMethods.Timestamp:
                    TimestampUpdatePack timestamPack = changedPack.ToTimestampUpdatePack(modelDef);
                    await UpdatePropertiesUsingTimestampAsync(modelDef, timestamPack, lastUser, transContext).ConfigureAwait(false);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        public async Task UpdatePropertiesAsync<T>(IList<PropertyChangePack> changedPacks, string lastUser, TransactionContext transContext) where T : BaseDbModel
        {
            if (changedPacks.IsNullOrEmpty())
            {
                return;
            }

            if (changedPacks.Count == 1)
            {
                await UpdatePropertiesAsync<T>(changedPacks.First(), lastUser, transContext).ConfigureAwait(false);
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();

            ThrowIfExceedMaxBatchNumber(changedPacks, lastUser, modelDef);

            //TODO: 是否允许不同的ConflictCheckMethod混杂?
            ConflictCheckMethods conflictCheckMethod = GetPropertyChangePackConflictCheckMethod(modelDef, changedPacks[0]);

            switch (conflictCheckMethod)
            {
                case ConflictCheckMethods.Ignore:
                    await UpdatePropertiesIgnoreConflictCheckAsync(
                        modelDef,
                        changedPacks.Select(cp => cp.ToIgnoreConflictCheckUpdatePack(modelDef)).ToList(),
                        lastUser,
                        transContext).ConfigureAwait(false);
                    break;

                case ConflictCheckMethods.OldNewValueCompare:
                    await UpdatePropertiesUsingOldNewCompareAsync(
                        modelDef,
                        changedPacks.Select(cp => cp.ToOldNewCompareUpdatePack(modelDef)).ToList(),
                        lastUser,
                        transContext).ConfigureAwait(false);
                    break;

                case ConflictCheckMethods.Timestamp:
                    await UpdatePropertiesUsingTimestampAsync(
                        modelDef,
                        changedPacks.Select(cp => cp.ToTimestampUpdatePack(modelDef)).ToList(),
                        lastUser,
                        transContext).ConfigureAwait(false);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion
    }
}