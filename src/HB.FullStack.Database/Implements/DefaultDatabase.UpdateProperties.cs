﻿/*
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
using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    internal partial class DefaultDatabase
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

            if (!modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.Timestamp))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow Timestamp Conflict Check.");
            }

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesTimestampCommand(modelDef, updatePack, lastUser);

                long rows = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    //没有这样的ID，或者版本冲突
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, $"没有这样的ID，或者使用Timestamp版本的乐观锁，出现冲突。UpdatePack:{SerializeUtil.ToJson(updatePack)}, lastUser:{lastUser}", "");
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

            if (!modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.Timestamp))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow Timestamp Conflict Check.");
            }

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesTimestampCommand(modelDef, updatePacks, lastUser);

                using IDataReader reader = transactionContext != null
                    ? await modelDef.Engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched == 1)
                    {
                    }
                    else if (matched == 0)
                    {
                        throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "UpdatePropertiesUsingTimestamp. 没有这样的ID，或者产生冲突！");
                    }
                    else
                    {
                        throw DbExceptions.FoundTooMuch(modelDef.FullName, $"UpdatePropertiesUsingTimestamp. Packs:{SerializeUtil.ToJson(updatePacks)}, ModelDef:{modelDef.FullName}, lastUser:{lastUser}");
                    }

                    count++;
                }

                if (count != updatePacks.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "UpdatePropertiesUsingTimestamp. 数量不对.");
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region OldNewCompare

        public Task UpdatePropertiesAsync<T>(OldNewCompareUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesUsingOldNewCompareAsync(modelDef, updatePack, lastUser, transContext);
        }

        public Task UpdatePropertiesAsync<T>(IList<OldNewCompareUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext) where T : BaseDbModel, new()
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

                if (matchedRows == 1)
                {
                    return;
                }
                else if (matchedRows == 0)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, $"没有这样的ID，或者使用新旧值对比的乐观锁出现冲突。UpdatePack:{updatePack}, lastUser:{lastUser}", "");
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

            transactionContext.ThrowIfNull(nameof(transactionContext));
            modelDef.ThrowIfNotWriteable();

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesOldNewCompareCommand(modelDef, updatePacks, lastUser);

                using IDataReader reader = transactionContext != null
                    ? await modelDef.Engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false) 
                    : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched == 1)
                    {
                    }
                    else if (matched == 0)
                    {
                        throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "UpdatePropertiesUsingOldNewCompareAsync. 没有这样的ID，或者产生冲突！");
                    }
                    else
                    {
                        throw DbExceptions.FoundTooMuch(modelDef.FullName, $"UpdatePropertiesUsingOldNewCompareAsync. Packs:{SerializeUtil.ToJson(updatePacks)}, ModelDef:{modelDef.FullName}, lastUser:{lastUser}");
                    }

                    count++;
                }

                if (count != updatePacks.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "UpdatePropertiesUsingOldNewCompareAsync, 数量不对");
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region IgnoreConflictCheck

        public Task UpdatePropertiesAsync<T>(IgnoreConflictCheckUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesIgnoreConflictCheckAsync(modelDef, updatePack, lastUser, transContext);
        }

        public Task UpdatePropertiesAsync<T>(IList<IgnoreConflictCheckUpdatePack> updatePacks, string lastUser, TransactionContext transContext) where T : BaseDbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            return UpdatePropertiesIgnoreConflictCheckAsync(modelDef, updatePacks, lastUser, transContext);
        }

        private async Task UpdatePropertiesIgnoreConflictCheckAsync(DbModelDef modelDef, IgnoreConflictCheckUpdatePack updatePack, string lastUser, TransactionContext? transContext)
        {
            updatePack.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable();

            if (!modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.Ignore))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow Ignore Conflict Check.");
            }

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateUpdatePropertiesIgnoreConflictCheckCommand(modelDef, updatePack, lastUser);

                long rows = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false) 
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DbExceptions.NotFound($"没有找到这样的ID。UpdatePack:{SerializeUtil.ToJson(updatePack)}, lastUser:{lastUser}, model:{modelDef.FullName}");
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

            transactionContext.ThrowIfNull(nameof(transactionContext));
            updatePacks.ThrowIfNotValid();
            modelDef.ThrowIfNotWriteable();

            if (!modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.Ignore))
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} disallow Ignore Conflict Check.");
            }

            try
            {
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdatePropertiesIgnoreConflictCheckCommand(modelDef, updatePacks, lastUser);

                using IDataReader reader = transactionContext != null
                    ? await modelDef.Engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false) 
                    : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched == 1)
                    {
                    }
                    else if (matched == 0)
                    {
                        throw DbExceptions.NotFound($"没有找到这样的ID。lastUser:{lastUser}, model:{modelDef.FullName}");
                    }
                    else
                    {
                        throw DbExceptions.FoundTooMuch(modelDef.FullName, $" lastUser:{lastUser}, model:{modelDef.FullName}");
                    }

                    count++;
                }

                if (count != updatePacks.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(updatePacks), "UpdatePropertiesIgnoreConflictCheckAsync. 数量不对.");
                }
            }
            catch (Exception ex)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(updatePacks), ex);
            }
        }

        #endregion

        #region PropertyChangePack

        private static DbConflictCheckMethods GetPropertyChangePackConflictCheckMethod(DbModelDef modelDef, PropertyChangePack changePack)
        {
            if (modelDef.BestConflictCheckMethodWhenUpdate == DbConflictCheckMethods.Timestamp && !changePack.ContainsProperty(nameof(ITimestamp.Timestamp)))
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

            return modelDef.BestConflictCheckMethodWhenUpdate;
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

        public async Task UpdatePropertiesAsync<T>(IList<PropertyChangePack> changedPacks, string lastUser, TransactionContext transContext) where T : BaseDbModel, new()
        {
            if (changedPacks.IsNullOrEmpty())
            {
                return;
            }

            if (changedPacks.Count == 1)
            {
                await UpdatePropertiesAsync<T>(changedPacks.First(), lastUser, transContext).ConfigureAwait(false);
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            //TODO: 是否允许不同的ConflictCheckMethod混杂?
            DbConflictCheckMethods conflictCheckMethod = GetPropertyChangePackConflictCheckMethod(modelDef, changedPacks[0]);

            switch (conflictCheckMethod)
            {
                case DbConflictCheckMethods.Ignore:
                    await UpdatePropertiesIgnoreConflictCheckAsync(
                        modelDef,
                        changedPacks.Select(cp => cp.ToIgnoreConflictCheckUpdatePack(modelDef)).ToList(),
                        lastUser,
                        transContext).ConfigureAwait(false);
                    break;

                case DbConflictCheckMethods.OldNewValueCompare:
                    await UpdatePropertiesUsingOldNewCompareAsync(
                        modelDef,
                        changedPacks.Select(cp => cp.ToOldNewCompareUpdatePack(modelDef)).ToList(),
                        lastUser,
                        transContext).ConfigureAwait(false);
                    break;

                case DbConflictCheckMethods.Timestamp:
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