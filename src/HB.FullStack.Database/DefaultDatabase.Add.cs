using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        /// <summary>
        /// 增加,并且item被重新赋值，反应Version变化
        /// </summary>
        public async Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, item);

            long oldTimestamp = -1;
            string oldLastUser = "";

            try
            {
                //Prepare
                if (item is TimestampDbModel serverModel)
                {
                    oldTimestamp = serverModel.Timestamp;
                    oldLastUser = serverModel.LastUser;

                    serverModel.Timestamp = TimeUtil.UtcNowTicks;
                    serverModel.LastUser = lastUser;
                }

                var command = DbCommandBuilder.CreateAddCommand(EngineType, modelDef, item);

                object? rt = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, modelDef.DatabaseName!, command, true).ConfigureAwait(false);

                if (modelDef.IsIdAutoIncrement)
                {
                    ((ILongId)item).Id = System.Convert.ToInt64(rt, CultureInfo.InvariantCulture);
                }
            }
            catch (DatabaseException ex)
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

                throw DatabaseExceptions.UnKown(type: modelDef.ModelFullName, item: SerializeUtil.ToJson(item), ex);
            }

            static void RestoreItem(T item, long oldTimestamp, string oldLastUser)
            {
                if (item is TimestampDbModel serverModel)
                {
                    serverModel.Timestamp = oldTimestamp;
                    serverModel.LastUser = oldLastUser;
                }
            }
        }

        /// <summary>
        /// BatchAddAsync，反应Version变化
        /// </summary>
        public async Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (_databaseEngine.DatabaseSettings.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchAdd超过批量操作的最大数目", items.Count(), lastUser);
            }

            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return new List<object>();
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser, items);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                //Prepare
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                IList<object> newIds = new List<object>();

                var command = DbCommandBuilder.CreateBatchAddCommand(EngineType, modelDef, items, transContext == null);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext?.Transaction,
                    modelDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                if (modelDef.IsIdAutoIncrement)
                {
                    while (reader.Read())
                    {
                        newIds.Add(reader.GetValue(0));
                    }

                    int num = 0;

                    foreach (var item in items)
                    {
                        ((ILongId)item).Id = System.Convert.ToInt64(newIds[num++], GlobalSettings.Culture);
                    }
                }
                else if (modelDef.IsIdGuid)
                {
                    foreach (var item in items)
                    {
                        newIds.Add(((IGuidId)item).Id);
                    }
                }
                else if (modelDef.IsIdLong)
                {
                    foreach (var item in items)
                    {
                        newIds.Add(((ILongId)item).Id);
                    }
                }

                return newIds;
            }
            catch (DatabaseException ex)
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

                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(items), ex);
            }

        }

    }
}
