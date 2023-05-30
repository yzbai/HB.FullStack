using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    internal partial class DefaultDatabase
    {
        /// <summary>
        /// AddOrUpdate,即override,不检查Timestamp
        /// </summary>
        public async Task AddOrUpdateByIdAsync<T>(T item, string lastUser, TransactionContext? transContext = null) where T : class, IDbModel
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(nameof(modelDef)).ThrowIfNotWriteable();
            
            ThrowIfNotAllowedIgnoreConflictCheck(modelDef);

            long? oldTimestamp = null;
            string? oldLastUser = "";

            try
            {
                PrepareItem(item, lastUser, ref oldLastUser, ref oldTimestamp);

                var command = DbCommandBuilder.CreateAddOrUpdateCommand(modelDef, item, false);

                _ = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                if (modelDef.IsPropertyTrackable)
                {
                    ReTrackIfTrackable2(item);
                }
            }
            catch (DbException ex)
            {
                if (transContext != null || ex.ComeFromEngine)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }
                throw;
            }
            catch (Exception ex) when (ex is not DbException)
            {
                if (transContext != null)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task AddOrUpdateByIdAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : class, IDbModel
        {
            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(nameof(modelDef)).ThrowIfNotWriteable();

            ThrowIfNotAllowedIgnoreConflictCheck(modelDef);

            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                var command = DbCommandBuilder.CreateBatchAddOrUpdateCommand(modelDef, items);

                _ = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                if (modelDef.IsPropertyTrackable)
                {
                    ReTrackIfTrackable2(items);
                }
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

                throw DbExceptions.UnKown(modelDef.FullName, $"Items:{SerializeUtil.ToJson(items)}", ex);
            }
        }
    }
}