using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        public async Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()
                .ThrowIfNull(typeof(T).FullName)
                .ThrowIfNotWriteable();


            long? oldTimestamp = null;
            string oldLastUser = "";

            try
            {
                PrepareItem(item, lastUser, ref oldLastUser, ref oldTimestamp);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);
                ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
                DbEngineCommand command = DbCommandBuilder.CreateAddCommand(modelDef, item);

                object? rt = transContext != null
                    ? await engine.ExecuteCommandScalarAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandScalarAsync(connectionString, command).ConfigureAwait(false);

                if (modelDef.IdType == DbModelIdType.AutoIncrementLongId)
                {
                    modelDef.PrimaryKeyPropertyDef.SetValueTo(item, System.Convert.ToInt64(rt, CultureInfo.InvariantCulture));
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
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw DbExceptions.UnKown(type: modelDef.ModelFullName, item: SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task AddAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel, new()
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
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);
                ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
                DbEngineCommand command = DbCommandBuilder.CreateBatchAddCommand(modelDef, items);

                using IDataReader reader = transContext != null
                    ? await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(connectionString, command);

                if (modelDef.IdType == DbModelIdType.AutoIncrementLongId)
                {
                    int num = 0;

                    while (reader.Read())
                    {
                        modelDef.PrimaryKeyPropertyDef.SetValueTo(items[num], reader.GetInt64(0));
                        ++num;
                    }
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

                throw DbExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(items), ex);
            }
        }
        
        private void ThrowIfExceedMaxBatchNumber<TObj>(IList<TObj> items, string lastUser, DbModelDef modelDef)
        {
            if (_dbSchemaManager.GetDbSchema(modelDef.DbSchemaName).MaxBatchNumber < items.Count)
            {
                throw DbExceptions.TooManyForBatch("BatchAdd超过批量操作的最大数目", items.Count, lastUser);
            }
        }

        private static void PrepareItem<T>(T item, string lastUser, ref string oldLastUser, ref long? oldTimestamp) where T : BaseDbModel, new()
        {
            if (item is ITimestamp timestampModel)
            {
                oldTimestamp = timestampModel.Timestamp;
                timestampModel.Timestamp = TimeUtil.Timestamp;
            }

            oldLastUser = item.LastUser;
            item.LastUser = lastUser;
        }

        private static void RestoreItem<T>(T item, long? oldTimestamp, string oldLastUser) where T : BaseDbModel, new()
        {
            if (item is ITimestamp timestampModel)
            {
                timestampModel.Timestamp = oldTimestamp!.Value;
            }

            item.LastUser = oldLastUser;
        }

        private static void PrepareBatchItems<T>(IList<T> items, string lastUser, List<long> oldTimestamps, List<string?> oldLastUsers, DbModelDef modelDef) where T : BaseDbModel, new()
        {
            long timestamp = TimeUtil.Timestamp;

            foreach (T item in items)
            {
                oldLastUsers.Add(item.LastUser);
                item.LastUser = lastUser;

                if (item is ITimestamp timestampModel)
                {
                    oldTimestamps.Add(timestampModel.Timestamp);

                    timestampModel.Timestamp = timestamp;
                }
            }
        }

        private static void RestoreBatchItems<T>(IList<T> items, IList<long> oldTimestamps, IList<string?> oldLastUsers, DbModelDef modelDef) where T : BaseDbModel, new()
        {
            for (int i = 0; i < items.Count; ++i)
            {
                T item = items[i];

                item.LastUser = oldLastUsers[i] ?? "";

                if (item is ITimestamp timestampModel)
                {
                    timestampModel.Timestamp = oldTimestamps[i];
                }
            }
        }
    }
}
