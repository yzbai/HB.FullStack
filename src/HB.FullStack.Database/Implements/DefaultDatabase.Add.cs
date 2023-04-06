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
        /// 增加,并且item被重新赋值，如有Timestamp，那么会被重新赋值当前最新的。
        /// </summary>
        public async Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));
            
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()
                .ThrowIfNull(typeof(T).FullName)
                .ThrowIfNotWriteable();

            //TruncateLastUser(ref lastUser);

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

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                var command = DbCommandBuilder.CreateAddCommand(modelDef, item);

                object? rt = transContext != null
                    ? await engine.ExecuteCommandScalarAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandScalarAsync(_dbSchemaManager.GetConnectionString(modelDef.DbSchemaName, true), command).ConfigureAwait(false);

                if (modelDef.IsIdAutoIncrement)
                {
                    ((ILongId)item).Id = System.Convert.ToInt64(rt, CultureInfo.InvariantCulture);
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
        /// AddAsync，反应Version变化
        /// </summary>
        public async Task<IEnumerable<object>> AddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (items.IsNullOrEmpty())
            {
                return new List<object>();
            }

            ThrowIf.NotValid(items, nameof(items));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(nameof(modelDef)).ThrowIfNotWriteable();

            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);
            //TruncateLastUser(ref lastUser);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                //Prepare
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                IList<object> newIds = new List<object>();

                var command = DbCommandBuilder.CreateBatchAddCommand(modelDef, items, transContext == null);

                using var reader = transContext != null
                    ? await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(_dbSchemaManager.GetConnectionString(modelDef.DbSchemaName, true), command);

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

        private void ThrowIfExceedMaxBatchNumber<TObj>(IEnumerable<TObj> items, string lastUser, DbModelDef modelDef)
        {
            if (_dbSchemaManager.GetDbSchema(modelDef.DbSchemaName).MaxBatchNumber < items.Count())
            {
                throw DbExceptions.TooManyForBatch("BatchAdd超过批量操作的最大数目", items.Count(), lastUser);
            }
        }

        private static void PrepareBatchItems<T>(IEnumerable<T> items, string lastUser, List<long> oldTimestamps, List<string?> oldLastUsers, DbModelDef modelDef) where T : DbModel, new()
        {
            if (!modelDef.IsTimestampDBModel)
            {
                return;
            }

            long timestamp = TimeUtil.Timestamp;

            foreach (var item in items)
            {
                if (item is TimestampDbModel tsItem)
                {
                    oldTimestamps.Add(tsItem.Timestamp);
                    oldLastUsers.Add(tsItem.LastUser);

                    tsItem.Timestamp = timestamp;
                    tsItem.LastUser = lastUser;
                }
            }
        }

        private static void RestoreBatchItems<T>(IEnumerable<T> items, IList<long> oldTimestamps, IList<string?> oldLastUsers, DbModelDef modelDef) where T : DbModel, new()
        {
            if (!modelDef.IsTimestampDBModel)
            {
                return;
            }

            for (int i = 0; i < items.Count(); ++i)
            {
                if (items.ElementAt(i) is TimestampDbModel tsItem)
                {
                    tsItem.Timestamp = oldTimestamps[i];
                    tsItem.LastUser = oldLastUsers[i] ?? "";
                }
            }
        }
    }
}
