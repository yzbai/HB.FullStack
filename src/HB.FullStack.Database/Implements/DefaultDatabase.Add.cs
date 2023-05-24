using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    internal partial class DefaultDatabase
    {
        public async Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()
                .ThrowIfNull(typeof(T).FullName)
                .ThrowIfNotWriteable();

            long? oldTimestamp = null;
            string? oldLastUser = "";

            try
            {
                SetPrimaryValueIfNull(new T[] { item }, modelDef);

                PrepareItem(item, lastUser, ref oldLastUser, ref oldTimestamp);

                DbEngineCommand command = DbCommandBuilder.CreateAddCommand(modelDef, item);

                object? rt = transContext != null
                    ? await modelDef.Engine.ExecuteCommandScalarAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandScalarAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);

                if (modelDef.IdType == DbModelIdType.AutoIncrementLongId)
                {
                    modelDef.PrimaryKeyPropertyDef.SetValueTo(item, System.Convert.ToInt64(rt, CultureInfo.InvariantCulture));
                }

                ReTrackIfTrackable(item, modelDef);
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

                throw DbExceptions.UnKown(type: modelDef.FullName, item: SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task AddAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel
        {
            if (items.IsNullOrEmpty())
            {
                return;
            }

            ThrowIf.Null(transContext, nameof(transContext));
            ThrowIf.NotValid(items, nameof(items));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(nameof(modelDef)).ThrowIfNotWriteable();

            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                SetPrimaryValueIfNull(items, modelDef);

                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                DbEngineCommand command = DbCommandBuilder.CreateBatchAddCommand(modelDef, items);

                using IDataReader reader = transContext != null
                    ? await modelDef.Engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command);

                if (modelDef.IdType == DbModelIdType.AutoIncrementLongId)
                {
                    int num = 0;

                    while (reader.Read())
                    {
                        modelDef.PrimaryKeyPropertyDef.SetValueTo(items[num], reader.GetInt64(0));
                        ++num;
                    }
                }

                ReTrackIfTrackable(items, modelDef);
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