using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new()
        {
            //if (item is IPropertyTrackableObject trackableObject)
            //{
            //    PropertyChangePack changePack = trackableObject.GetPropertyChangePack();

            //    await UpdatePropertiesAsync<T>(changePack, lastUser, transContext).ConfigureAwait(false);

            //    return;
            //}

            ThrowIf.NotValid(item, nameof(item));
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();
            DbConflictCheckMethods conflictCheckMethods = modelDef.BestConflictCheckMethodWhenUpdateEntire;

            if (conflictCheckMethods == DbConflictCheckMethods.OldNewValueCompare)
            {
                IPropertyTrackableObject trackableModel = item as IPropertyTrackableObject
                    ?? throw DbExceptions.ConflictCheckError($"{modelDef.FullName} using old new value compare method update whole, but not a IPropertyTrackable Object.");

                PropertyChangePack changePack = trackableModel.GetPropertyChangePack();

                await UpdatePropertiesAsync<T>(changePack, lastUser, transContext).ConfigureAwait(false);
                
                //trackableModel.Clear();

                return;
            }

            long? oldTimestamp = null;
            string oldLastUser = "";

            try
            {
                PrepareItem(item, lastUser, ref oldLastUser, ref oldTimestamp);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);
                ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
                DbEngineCommand command = DbCommandBuilder.CreateUpdateCommand(modelDef, item, oldTimestamp, conflictCheckMethods);

                long rows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(connectionString, command).ConfigureAwait(false);

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

                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(item), "");
                }

                throw DbExceptions.FoundTooMuch(modelDef.FullName, SerializeUtil.ToJson(item));
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


            //if (modelDef.IsPropertyTrackable)
            //{
            //    await UpdatePropertiesAsync<T>(
            //        items.Select(t => ((IPropertyTrackableObject)t).GetPropertyChangePack()).ToList(),
            //        lastUser,
            //        transContext).ConfigureAwait(false);

            //    return;
            //}


            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);
                ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
                DbEngineCommand command = DbCommandBuilder.CreateBatchUpdateCommand(modelDef, items, oldTimestamps);

                using IDataReader reader = transContext != null
                    ? await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(connectionString, command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(items), "BatchUpdate");
                    }

                    count++;
                }

                if (count != items.Count)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(items), "BatchUpdate");
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

                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(items), ex);
            }
        }
    }
}
