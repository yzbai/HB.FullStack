using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    internal partial class DefaultDatabase
    {
        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();
            ConflictCheckMethods bestConflictCheckMethod = modelDef.BestConflictCheckMethodWhenUpdate;

            if (bestConflictCheckMethod == ConflictCheckMethods.OldNewValueCompare)
            {
                IPropertyTrackableObject trackableModel = item as IPropertyTrackableObject
                    ?? throw DbExceptions.ConflictCheckError($"{modelDef.FullName} using old new value compare method update whole, but not a IPropertyTrackable Object.");

                PropertyChangePack changePack = trackableModel.GetPropertyChangePack();

                await UpdatePropertiesAsync<T>(changePack, lastUser, transContext).ConfigureAwait(false);

                //trackableModel.Clear();

                return;
            }

            //if (bestConflictCheckMethod != DbConflictCheckMethods.Ignore && bestConflictCheckMethod != DbConflictCheckMethods.Timestamp)
            //{
            //    throw DbExceptions.ConflictCheckError($"{modelDef.FullName} has wrong Best Conflict Check Method When update entire model: {bestConflictCheckMethod}.");
            //}

            long? oldTimestamp = null;
            string? oldLastUser = "";

            try
            {
                PrepareItem(item, lastUser, ref oldLastUser, ref oldTimestamp);

                DbEngineCommand command = bestConflictCheckMethod == ConflictCheckMethods.Timestamp
                    ? DbCommandBuilder.CreateUpdateTimestampCommand(modelDef, item, oldTimestamp!.Value)
                    : DbCommandBuilder.CreateUpdateIgnoreConflictCheckCommand(modelDef, item);

                long rows = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatch(modelDef, rows, item, lastUser);
            }
            catch (DbException ex)
            {
                if (transContext != null || ex.ComeFromEngine)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task UpdateAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel, new()
        {
            if (items.IsNullOrEmpty())
            {
                return;
            }

            ThrowIf.Null(transContext, nameof(transContext));
            ThrowIf.NotValid(items, nameof(items));

            if (items.Count == 1)
            {
                await UpdateAsync(items.First(), lastUser, transContext).ConfigureAwait(false);
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();
            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);

            ConflictCheckMethods bestConflictCheckMethod = modelDef.BestConflictCheckMethodWhenUpdate;

            if (bestConflictCheckMethod == ConflictCheckMethods.OldNewValueCompare)
            {
                if (!modelDef.IsPropertyTrackable)
                {
                    throw DbExceptions.ConflictCheckError($"{modelDef.FullName} using old new value compare method update whole, but not a IPropertyTrackable Object.");
                }

                var propertyChangePacks = items.Select(m => (m as IPropertyTrackableObject)!.GetPropertyChangePack()).ToList();

                await UpdatePropertiesAsync<T>(propertyChangePacks, lastUser, transContext).ConfigureAwait(false);

                return;
            }

            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                DbEngineCommand command = bestConflictCheckMethod == ConflictCheckMethods.Timestamp
                    ? DbCommandBuilder.CreateBatchUpdateTimestampCommand(modelDef, items, oldTimestamps)
                    : DbCommandBuilder.CreateBatchUpdateIgnoreConflictCheckCommand(modelDef, items, oldTimestamps);

                using IDataReader reader = transContext != null
                    ? await modelDef.Engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                CheckFoundMatches(modelDef, reader, items, lastUser);
            }
            catch (DbException ex)
            {
                if (transContext != null || ex.ComeFromEngine)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(items), ex);
            }
        }
    }
}