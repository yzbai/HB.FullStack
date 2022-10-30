using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

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

            DbModelDef modelDef = DefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);
            TruncateLastUser(ref lastUser);

            long oldTimestamp = -1;
            string oldLastUser = "";

            try
            {
                //Prepare
                if (item is TimestampDbModel serverModel)
                {
                    oldTimestamp = serverModel.Timestamp;
                    oldLastUser = serverModel.LastUser;

                    serverModel.Timestamp = TimeUtil.Timestamp;
                    serverModel.LastUser = lastUser;
                }

                IDatabaseEngine engine = DbManager.GetDatabaseEngine(modelDef);

                var command = DbCommandBuilder.CreateAddCommand(modelDef, item);

                object? rt = transContext != null
                    ? await engine.ExecuteCommandScalarAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandScalarAsync(DbManager.GetConnectionString(modelDef, true), command).ConfigureAwait(false);

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
            if (items.IsNullOrEmpty())
            {
                return new List<object>();
            }

            ThrowIf.NotValid(items, nameof(items));

            DbModelDef modelDef = DefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);
            ThrowIfTooMuchItems(items, lastUser, modelDef);
            TruncateLastUser(ref lastUser);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                IDatabaseEngine engine = DbManager.GetDatabaseEngine(modelDef);

                //Prepare
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                IList<object> newIds = new List<object>();

                var command = DbCommandBuilder.CreateBatchAddCommand(modelDef, items, transContext == null);

                using var reader = transContext != null
                    ? await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(DbManager.GetConnectionString(modelDef, true), command);

                if (modelDef.IsIdAutoIncrement)
                {
                    while (reader.Read())
                    {
                        newIds.Add(reader.GetValue(0));
                    }

                    int num = 0;

                    foreach (var item in items)
                    {
                        ((ILongId)item).Id = System.Convert.ToInt64(newIds[num++], Globals.Culture);
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

        private void ThrowIfTooMuchItems<TObj>(IEnumerable<TObj> items, string lastUser, DbModelDef modelDef)
        {
            DbSetting dbSetting = DbManager.GetDbSetting(modelDef);

            if (dbSetting.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchAdd超过批量操作的最大数目", items.Count(), lastUser);
            }
        }
    }
}
