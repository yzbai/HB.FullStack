using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Extensions;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (item is IPropertyTrackableObject trackableObject)
            {
                PropertyChangePack changePack = trackableObject.GetPropertyChanges();

                await UpdatePropertiesAsync<T>(changePack, lastUser, transContext).ConfigureAwait(false);

                return;
            }

            ThrowIf.NotValid(item, nameof(item));
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();

            //TruncateLastUser(ref lastUser);

            long oldTimestamp = -1;
            string oldLastUser = "";

            try
            {
                //Prepare
                if (item is TimestampDbModel timestampDBModel)
                {
                    oldTimestamp = timestampDBModel.Timestamp;
                    oldLastUser = timestampDBModel.LastUser;

                    timestampDBModel.Timestamp = TimeUtil.Timestamp;
                    timestampDBModel.LastUser = lastUser;
                }

                DbEngineCommand command = DbCommandBuilder.CreateUpdateCommand(modelDef, item, oldTimestamp);

                var engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                long rows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(_dbSchemaManager.GetConnectionString(modelDef.DbSchemaName, true), command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    //TODO: 这里返回0，一般是因为version不匹配，单也有可能是Id不存在，或者Deleted=1.
                    //可以改造SqlHelper中的update语句为如下，进行一般改造，排除上述可能。
                    //在原始的update语句，比如：update tb_userdirectorypermission set LastUser='TTTgdTTTEEST' where Id = uuid_to_bin('08da5b35-b123-2d4f-876c-6ee360db28c1') and Deleted = 0 and Version='0';
                    //后面select found_rows(), count(1) as 'exits' from tb_userdirectorypermission where Id = uuid_to_bin('08da5b35-b123-2d4f-876c-6ee360db28c1') and Deleted = 0;
                    //然后使用Reader读取，通过两个值进行判断。
                    //如果found_rows=1，正常返回
                    //如果found_rows=0, exists = 1, version冲突
                    //如果found_rows=0, exists = 0, 已删除

                    //不存在和Version冲突，统称为冲突，所以不用改，反正后续业务为了解决冲突也会重新select什么的，到时候可以判定是已经删掉了还是version冲突

                    throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(item), "");
                }

                throw DbExceptions.FoundTooMuch(modelDef.ModelFullName, SerializeUtil.ToJson(item));
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

                throw DbExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }

            static void RestoreItem(T item, long oldTimestamp, string oldLastUser)
            {
                if (item is TimestampDbModel timestampDBModel)
                {
                    timestampDBModel.Timestamp = oldTimestamp;
                    timestampDBModel.LastUser = oldLastUser;
                }
            }
        }

        public async Task UpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (items.IsNullOrEmpty())
            {
                return;
            }

            if (items.Count() == 1)
            {
                await UpdateAsync(items.First(), lastUser, transContext).ConfigureAwait(false);
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();

            if (modelDef.IsPropertyTrackable)
            {
                await UpdatePropertiesAsync<T>(
                    items.Select(t => ((IPropertyTrackableObject)t).GetPropertyChanges()).ToList(), 
                    lastUser, 
                    transContext).ConfigureAwait(false);

                return;
            }

            ThrowIf.NotValid(items, nameof(items));

            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);
            //TruncateLastUser(ref lastUser);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                var command = DbCommandBuilder.CreateBatchUpdateCommand(modelDef, items, oldTimestamps, transContext == null);

                var engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                using var reader = transContext != null
                    ? await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(_dbSchemaManager.GetConnectionString(modelDef.DbSchemaName, true), command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(items), "BatchUpdate");
                    }

                    count++;
                }

                if (count != items.Count())
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(items), "BatchUpdate");
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
    }
}
